\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    ALTER TABLE equivocation ALTER COLUMN consensus_step TYPE text;

    INSERT INTO database_version (version_number, description)
    VALUES (11, 'Change type of consensus_step in equivocation table');
END
$$;
