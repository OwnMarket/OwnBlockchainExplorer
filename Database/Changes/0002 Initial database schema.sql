\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN
    -- TODO: Implement proper initial database design

    CREATE TABLE address (
        address_id BIGSERIAL NOT NULL,
        blockchain_address VARCHAR(50) NOT NULL,
        chx_balance DECIMAL(18, 7) NOT NULL,
        nonce BIGINT NOT NULL,

        CONSTRAINT address__pk PRIMARY KEY (address_id),
        CONSTRAINT address__uk__blockchain_address UNIQUE (blockchain_address)
    );

    INSERT INTO database_version (version_number, description)
    VALUES (2, 'Initial database schema');
END
$$;
