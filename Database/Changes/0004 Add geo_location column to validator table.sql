\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    -- DROP FK constraints validator
    ALTER TABLE block DROP CONSTRAINT block_validator_fk;

    -- RENAME table to old
    ALTER TABLE validator RENAME TO validator_old;

    -- RENAME constraints in the old tables
    ALTER TABLE validator_old RENAME CONSTRAINT validator_pk TO validator_pk_old;
    ALTER SEQUENCE validator_validator_id_seq RENAME TO validator_validator_id_seq_old;

   	CREATE TABLE validator (
		validator_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		blockchain_address text NOT NULL,
		network_address text NULL,
		geo_location json NULL,
		shared_reward_percent numeric NOT NULL,
		is_active bool NOT NULL,
		is_deleted bool NOT NULL,

		CONSTRAINT validator_pk PRIMARY KEY (validator_id)
	);

    -- COPY data from old table
    INSERT INTO validator (validator_id, blockchain_address, network_address, geo_location, shared_reward_percent, is_active, is_deleted)
    OVERRIDING SYSTEM VALUE
    SELECT validator_id, blockchain_address, network_address, NULL, shared_reward_percent, is_active, is_deleted FROM validator_old;

    -- DROP old table
    DROP TABLE validator_old;

    -- RESYNC PK
    PERFORM setval('validator_validator_id_seq', (SELECT MAX(validator_id) FROM validator) + 1);


    -- RECREATE FK constraints validator
    ALTER TABLE block ADD CONSTRAINT block_validator_fk FOREIGN KEY (validator_id) REFERENCES validator (validator_id);

    INSERT INTO database_version (version_number, description)
    VALUES (4, 'Add geo_location column to validator table');
END
$$;
