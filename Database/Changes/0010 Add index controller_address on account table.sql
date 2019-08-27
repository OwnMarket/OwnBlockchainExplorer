\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    DROP INDEX IF EXISTS account__ix__controller_address;
    CREATE INDEX account__ix__controller_address ON account (controller_address);


    INSERT INTO database_version (version_number, description)
    VALUES (10, 'Add index controller_address on account table');
END
$$;
