\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    DROP INDEX IF EXISTS account__ix__hash;

    DROP INDEX IF EXISTS address__ix__blockchain_address;

    DROP INDEX IF EXISTS asset__ix__hash;

    DROP INDEX IF EXISTS block__ix__block_number;

    DROP INDEX IF EXISTS equivocation__ix__equivocation_proof_hash;

    DROP INDEX IF EXISTS transaction__ix__hash;

    DROP INDEX IF EXISTS validator__ix__blockchain_address;

    INSERT INTO database_version (version_number, description)
    VALUES (8, 'Remove redundant indexes on hash columns');
END
$$;
