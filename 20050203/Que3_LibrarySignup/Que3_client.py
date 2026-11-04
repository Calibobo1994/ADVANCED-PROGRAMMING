from __future__ import annotations

import argparse
import json
import os
import re
import socket
import sys

DEFAULT_HOST = os.environ.get("QUE3_HOST", "localhost")
DEFAULT_PORT = int(os.environ.get("QUE3_PORT", "5050"))

emailPattern = re.compile(r"^[^@\s]+@[^@\s]+\.[^@\s]+$")
contactPattern = re.compile(r"^\+?[0-9][0-9\s-]{5,18}[0-9]$")

def get_input(prompt: str) -> str:
    while True:
        try:
            value = input(prompt).strip()
        except EOFError:
            print("\nInput closed.", file=sys.stderr)
            raise SystemExit(1)
        if value:
            return value
        print("This field cannot be empty.")

def get_contact() -> str:
    while True:
        value = get_input("Contact number: ")
        digits = re.sub(r"\D", "", value)
        if contactPattern.match(value) and 7 <= len(digits) <= 15:
            return value
        print("Enter a contact number with 7 to 15 digits (spaces, + and - are allowed).")

def get_email() -> str:
    while True:
        value = get_input("Email address: ")
        if emailPattern.match(value):
            return value
        print("Enter a valid email address, e.g. name@example.com.")

def get_months() -> int:
    while True:
        value = get_input("Membership duration (6 or 12 months): ")
        if value in ("6", "12"):
            return int(value)
        print("Enter exactly 6 or 12.")

def get_details() -> dict:
    print("Library sign-up")
    print("---------------")
    return {
        "full_name": get_input("Full name: "),
        "address": get_input("Residential address: "),
        "contact_number": get_contact(),
        "email": get_email(),
        "membership_months": get_months(),
    }

def send_to_server(data: dict, host: str, port: int) -> dict:
    request = (json.dumps(data) + "\n").encode("utf-8")

    with socket.create_connection((host, port), timeout=10) as sock:
        sock.sendall(request)

        reply = b""
        while not reply.endswith(b"\n"):
            chunk = sock.recv(4096)
            if not chunk:
                break
            reply += chunk

    if not reply.strip():
        raise ValueError("the server closed the connection without replying")

    return json.loads(reply.decode("utf-8").strip())

def get_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Library sign-up TCP client")
    parser.add_argument("--host", default=DEFAULT_HOST, help="server address")
    parser.add_argument("--port", type=int, default=DEFAULT_PORT, help="server port")
    return parser.parse_args()

def main() -> int:
    args = get_args()
    data = get_details()

    try:
        reply = send_to_server(data, args.host, args.port)
    except ConnectionRefusedError:
        print(
            f"Could not connect to the server at {args.host}:{args.port}. "
            "Make sure Que3_server.py is running.",
            file=sys.stderr,
        )
        return 1
    except (socket.timeout, TimeoutError):
        print(f"The server at {args.host}:{args.port} did not reply in time.", file=sys.stderr)
        return 1
    except (OSError, ValueError) as error:
        print(f"Communication error: {error}", file=sys.stderr)
        return 1

    if reply.get("status") == "ok" and reply.get("registration_no"):
        print()
        print("Sign-up successful.")
        print(f"Your registration number is: {reply['registration_no']}")
        print("Keep this number for all future interactions.")
        return 0

    print(f"Sign-up failed: {reply.get('message', 'unknown server error')}", file=sys.stderr)
    return 1

if __name__ == "__main__":
    sys.exit(main())
