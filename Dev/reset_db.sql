\set ON_ERROR_STOP on

\c postgres

DROP DATABASE IF EXISTS own_blockchain_explorer;
DROP ROLE IF EXISTS own_blockchain_explorer_api;
DROP ROLE IF EXISTS own_blockchain_explorer_scanner;

\ir '../Database/1_init_server.sql'
\ir '../Database/2_create_db.sql'
\ir '../Database/3_apply_db_changes.sql'
