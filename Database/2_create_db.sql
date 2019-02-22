\set ON_ERROR_STOP on

\c postgres

CREATE DATABASE own_blockchain_explorer;
\c own_blockchain_explorer

SET search_path TO public;

-- Create extensions
--CREATE EXTENSION adminpack;
CREATE EXTENSION pgcrypto; -- UUID (Guid) Support

-- Create schemas
CREATE SCHEMA IF NOT EXISTS own;

-- Set default permissions
ALTER DEFAULT PRIVILEGES
GRANT SELECT ON TABLES TO own_blockchain_explorer_api;
ALTER DEFAULT PRIVILEGES
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO own_blockchain_explorer_scanner;

ALTER DEFAULT PRIVILEGES
GRANT SELECT ON SEQUENCES TO own_blockchain_explorer_api;
ALTER DEFAULT PRIVILEGES
GRANT SELECT, USAGE ON SEQUENCES TO own_blockchain_explorer_scanner;

-- Set permissions on schemas
GRANT ALL ON SCHEMA public TO postgres;
GRANT USAGE ON SCHEMA public TO own_blockchain_explorer_api;
GRANT USAGE ON SCHEMA public TO own_blockchain_explorer_scanner;

GRANT ALL ON SCHEMA own TO postgres;
GRANT USAGE ON SCHEMA own TO own_blockchain_explorer_api;
GRANT USAGE ON SCHEMA own TO own_blockchain_explorer_scanner;
