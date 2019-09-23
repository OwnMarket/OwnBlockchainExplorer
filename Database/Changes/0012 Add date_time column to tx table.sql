\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    -- DROP FK constraints
    ALTER TABLE blockchain_event DROP CONSTRAINT blockchain_event_transaction_fk;

    -- RENAME table to old
    ALTER TABLE "transaction" RENAME TO transaction_old;

    -- RENAME constraints in the old tables
    ALTER TABLE transaction_old RENAME CONSTRAINT transaction_pk TO transaction_pk_old;
    ALTER SEQUENCE transaction_transaction_id_seq RENAME TO transaction_transaction_id_seq_old;

    CREATE TABLE "transaction" (
        transaction_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
        hash text NOT NULL,
        nonce int8 NOT NULL,
        "timestamp" int8 NOT NULL,
        "date_time" timestamp NOT NULL,
        expiration_time timestamp NULL,
        action_fee numeric NOT NULL,
        status text NOT NULL,
        error_message text NULL,
        failed_action_number int2 NULL,

        CONSTRAINT transaction_pk PRIMARY KEY (transaction_id)
    );

    -- COPY data from old table
    INSERT INTO "transaction" (transaction_id, hash, nonce, timestamp, date_time, expiration_time, action_fee, status, error_message, failed_action_number)
    OVERRIDING SYSTEM VALUE
    SELECT transaction_id, hash, nonce, timestamp, (TIMESTAMP 'epoch' + trunc(timestamp/1000) * INTERVAL '1 second')::timestamp as date_time, expiration_time, action_fee, status, error_message, failed_action_number FROM transaction_old;

    -- DROP old table
    DROP TABLE transaction_old;

    -- RESYNC PK
    PERFORM setval('transaction_transaction_id_seq', (SELECT MAX(transaction_id) FROM "transaction") + 1);


    -- RECREATE FK constraints
    ALTER TABLE blockchain_event ADD CONSTRAINT blockchain_event_transaction_fk FOREIGN KEY (transaction_id) REFERENCES "transaction" (transaction_id);

    INSERT INTO database_version (version_number, description)
    VALUES (12, 'Add date_time column to tx table');
END
$$;
