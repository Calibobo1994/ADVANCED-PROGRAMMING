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
