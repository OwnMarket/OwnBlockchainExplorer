\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

	ALTER TABLE "validator" ADD is_deleted bool NOT NULL;
	
	ALTER TABLE blockchain_event ALTER COLUMN fee TYPE numeric using fee::numeric;
	
	ALTER TABLE "transaction" ALTER COLUMN "timestamp" TYPE int8 using EXTRACT(EPOCH FROM "timestamp");
	
	ALTER TABLE block ALTER COLUMN previous_block_hash DROP NOT NULL;
	ALTER TABLE block ALTER COLUMN previous_block_id DROP NOT NULL;
	
    INSERT INTO database_version (version_number, description)
    VALUES (3, 'Minor changes on validator, event, tx and block tables');
END
$$;
