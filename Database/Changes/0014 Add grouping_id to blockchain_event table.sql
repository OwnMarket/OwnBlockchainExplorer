\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    ALTER TABLE blockchain_event ADD COLUMN grouping_id uuid;

    INSERT INTO database_version (version_number, description)
    VALUES (14, 'Add grouping_id to blockchain_event table');
END
$$;
