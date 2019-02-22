\set ON_ERROR_STOP on

\c postgres

CREATE USER own_blockchain_explorer_api WITH PASSWORD 'testpass1';
CREATE USER own_blockchain_explorer_scanner WITH PASSWORD 'testpass1';

-- Password should be changed on real environment
--ALTER USER own_blockchain_explorer_api WITH PASSWORD 'XXXXXXXXXX';
