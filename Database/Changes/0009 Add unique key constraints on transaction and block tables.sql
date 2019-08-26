\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    ALTER TABLE block ADD CONSTRAINT block__uk__block_number UNIQUE (block_number);
    ALTER TABLE block ADD CONSTRAINT block__uk__hash UNIQUE (hash);
    ALTER TABLE block ADD CONSTRAINT block__uk__timestamp UNIQUE ("timestamp");
    ALTER TABLE transaction ADD CONSTRAINT transaction__uk__hash UNIQUE (hash);

    INSERT INTO database_version (version_number, description)
    VALUES (9, 'Add unique key constraints on transaction and block tables');
END
$$;
