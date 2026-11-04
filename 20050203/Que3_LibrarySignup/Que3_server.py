from __future__ import annotations

import argparse
import json
import os
import re
import signal
import socket
import socketserver
import sys
import threading
import uuid

import psycopg2

defaultHost = os.environ.get("QUE3_HOST", "localhost")
defaultPort = int(os.environ.get("QUE3_PORT", "5050"))

dbConfig = {
    "host": os.environ.get("PGHOST", "localhost"),
    "port": int(os.environ.get("PGPORT", "5432")),
    "dbname": os.environ.get("PGDATABASE", "callistus"),
    "user": os.environ.get("PGUSER", "postgres"),
    "password": os.environ.get("PGPASSWORD", "postgres"),
}

SCHEMA = """
CREATE TABLE IF NOT EXISTS library_members (
    id                SERIAL PRIMARY KEY,
    registration_no   TEXT UNIQUE NOT NULL,
    full_name         TEXT NOT NULL,
    address           TEXT NOT NULL,
    contact_number    TEXT NOT NULL,
    email             TEXT NOT NULL,
    membership_months INTEGER NOT NULL CHECK (membership_months IN (6, 12)),
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now()
);
"""

emailPattern = re.compile(r"^[^@\s]+@[^@\s]+\.[^@\s]+$")
contactPattern = re.compile(r"^\+?[0-9][0-9\s-]{5,18}[0-9]$")

def clean_text(value: object) -> str:
    if not isinstance(value, str):
        raise ValueError("must be text")
    cleaned = value.strip()
    if not cleaned:
        raise ValueError("cannot be empty")
    return cleaned

def check_request(data: dict) -> dict:
    required = ["full_name", "address", "contact_number", "email", "membership_months"]
    missing = [field for field in required if field not in data]
    if missing:
        raise ValueError(f"missing field(s): {', '.join(missing)}")

    member = {
        "full_name": clean_text(data["full_name"]),
        "address": clean_text(data["address"]),
        "contact_number": clean_text(data["contact_number"]),
        "email": clean_text(data["email"]),
    }

    contact = member["contact_number"]
    digits = re.sub(r"\D", "", contact)
    if not contactPattern.match(contact) or not 7 <= len(digits) <= 15:
        raise ValueError("contact number must contain 7 to 15 digits")

    if not emailPattern.match(member["email"]):
        raise ValueError("email address is not valid")

    months = data["membership_months"]
    if isinstance(months, bool) or not isinstance(months, int) or months not in (6, 12):
        raise ValueError("membership_months must be 6 or 12")

    member["membership_months"] = months
    return member

def create_table() -> None:
    with psycopg2.connect(**dbConfig) as conn:
        with conn.cursor() as cur:
            cur.execute(SCHEMA)

def save_to_database(member: dict) -> str:
    with psycopg2.connect(**dbConfig) as conn:
        with conn.cursor() as cur:
            cur.execute(
                """
                INSERT INTO library_members
                    (registration_no, full_name, address,
                     contact_number, email, membership_months)
                VALUES (%s, %s, %s, %s, %s, %s)
                RETURNING id
                """,
                (
                    "TEMP-" + uuid.uuid4().hex,
                    member["full_name"],
                    member["address"],
                    member["contact_number"],
                    member["email"],
                    member["membership_months"],
                ),
            )
            new_id = cur.fetchone()[0]

            regNo = f"LIB-{new_id:06d}"
            cur.execute(
                "UPDATE library_members SET registration_no = %s WHERE id = %s",
                (regNo, new_id),
            )

    return regNo

class ClientHandler(socketserver.StreamRequestHandler):

    def handle(self) -> None:
        clientAddress = f"{self.client_address[0]}:{self.client_address[1]}"
        print_log(f"connection from {clientAddress}")

        try:
            raw = self.rfile.readline()
            if not raw:
                print_log(f"{clientAddress} closed the connection without sending data")
                return

            text = raw.decode("utf-8").strip()
            print_log(f"{clientAddress} payload: {text}")

            try:
                data = json.loads(text)
                if not isinstance(data, dict):
                    raise ValueError("request must be a JSON object")
                member = check_request(data)
                regNo = save_to_database(member)
            except ValueError as error:
                response = {"status": "error", "message": str(error)}
                print_log(f"{clientAddress} rejected: {error}")
            except psycopg2.Error as error:
                response = {"status": "error", "message": "database error, please try again"}
                print_log(f"{clientAddress} database error: {error}")
            else:
                response = {"status": "ok", "registration_no": regNo}
                print_log(f"{clientAddress} stored {regNo}")

            self.wfile.write((json.dumps(response) + "\n").encode("utf-8"))
        except (ConnectionError, UnicodeDecodeError) as error:
            print_log(f"{clientAddress} connection error: {error}")

class Server(socketserver.ThreadingTCPServer):

    allow_reuse_address = True
    daemon_threads = True

_log_lock = threading.Lock()

def print_log(message: str) -> None:
    with _log_lock:
        print(message, flush=True)

def get_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Library sign-up TCP server")
    parser.add_argument("--host", default=defaultHost, help="address to listen on")
    parser.add_argument("--port", type=int, default=defaultPort, help="TCP port to listen on")
    return parser.parse_args()

def main() -> int:
    args = get_args()

    try:
        create_table()
    except psycopg2.Error as error:
        print(f"Could not connect to PostgreSQL or create the table: {error}", file=sys.stderr)
        print(
            "Check PGHOST, PGPORT, PGDATABASE, PGUSER and PGPASSWORD, "
            "and make sure the database exists.",
            file=sys.stderr,
        )
        return 1

    server = Server((args.host, args.port), ClientHandler)

    def stop_server(_signum=None, _frame=None) -> None:
        print_log("\nShutting down...")
        threading.Thread(target=server.shutdown, daemon=True).start()

    signal.signal(signal.SIGINT, stop_server)
    if hasattr(signal, "SIGTERM"):
        signal.signal(signal.SIGTERM, stop_server)

    print_log(f"Library server listening on {args.host}:{args.port}")
    print_log(f"Database: {dbConfig['user']}@{dbConfig['host']}:{dbConfig['port']}/{dbConfig['dbname']}")
    print_log("Press Ctrl+C to stop.")

    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
        print_log("Server stopped.")

    return 0

if __name__ == "__main__":
    sys.exit(main())
