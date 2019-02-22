\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN
    CREATE TABLE database_version (
        version_number smallint NOT NULL,
        description text NOT NULL,
        release_time timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),

        CONSTRAINT database_version__pk PRIMARY KEY (version_number)
    );

    INSERT INTO database_version (version_number, description)
    VALUES (1, 'Add database_version table');
END
$$;
