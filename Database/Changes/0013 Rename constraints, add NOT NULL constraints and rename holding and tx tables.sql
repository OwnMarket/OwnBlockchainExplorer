\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

    -- Add NOT NULL constraints
    UPDATE asset
    SET is_eligibility_required = false
    WHERE is_eligibility_required IS NULL;

    ALTER TABLE asset ALTER COLUMN is_eligibility_required SET NOT NULL;
    ALTER TABLE block ALTER COLUMN previous_block_hash SET NOT NULL;
    ALTER TABLE block ALTER COLUMN consensus_round SET NOT NULL;
    ALTER TABLE equivocation ALTER COLUMN signature_1 SET NOT NULL;
    ALTER TABLE equivocation ALTER COLUMN signature_2 SET NOT NULL;
    ALTER TABLE tx_action ALTER COLUMN action_data SET NOT NULL;

    -- Alter column type
    ALTER TABLE tx_action ALTER COLUMN action_data TYPE json USING action_data::json;

    -- Alter PK constraint names
    ALTER TABLE account RENAME CONSTRAINT account_pk TO account__pk;
    ALTER TABLE address RENAME CONSTRAINT address_pk TO address__pk;
    ALTER TABLE asset RENAME CONSTRAINT asset_pk TO asset__pk;
    ALTER TABLE block RENAME CONSTRAINT block_pk TO block__pk;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_pk TO blockchain_event__pk;
    ALTER TABLE equivocation RENAME CONSTRAINT equivocation_pk TO equivocation__pk;
    ALTER TABLE tx_action RENAME CONSTRAINT tx_action_pk TO tx_action__pk;
    ALTER TABLE validator RENAME CONSTRAINT validator_pk TO validator__pk;

    -- Alter FK constraint names
    ALTER TABLE block RENAME CONSTRAINT block_block_fk TO block__fk__block;
    ALTER TABLE block RENAME CONSTRAINT block_validator_fk TO block__fk__validator;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_account_fk TO blockchain_event__fk__account;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_address_fk TO blockchain_event__fk__address;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_asset_fk TO blockchain_event__fk__asset;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_block_fk TO blockchain_event__fk__block;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_equivocation_fk TO blockchain_event__fk__equivocation;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_tx_action_fk TO blockchain_event__fk__tx_action;
    ALTER TABLE equivocation RENAME CONSTRAINT equivocation_block_fk TO equivocation__fk__block;

    -- Alter UK constraint names
    ALTER TABLE address RENAME CONSTRAINT address__uk__address TO address__uk__blockchain_address;
    ALTER TABLE validator RENAME CONSTRAINT validator__uk_address TO validator__uk__blockchain_address;

    -- Add CHECK constraint
    ALTER TABLE block ADD CONSTRAINT block__ck__previous_block_id CHECK (previous_block_id IS NOT NULL OR block_number = 0);

    -- Rename transaction table into tx
    ALTER TABLE blockchain_event RENAME COLUMN transaction_id TO tx_id;
    ALTER TABLE blockchain_event RENAME CONSTRAINT blockchain_event_transaction_fk TO blockchain_event__fk__tx;
    ALTER TABLE "transaction" RENAME TO tx;
    ALTER TABLE tx RENAME COLUMN transaction_id TO tx_id;
    ALTER TABLE tx RENAME CONSTRAINT transaction_pk TO tx__pk;
    ALTER SEQUENCE transaction_transaction_id_seq RENAME TO tx_tx_id_seq;
    ALTER INDEX blockchain_event__ix__transaction_id RENAME TO blockchain_event__ix__tx_id;

    -- Rename holding_eligibility table into holding
    DROP INDEX holding_eligibility__ix__account_id;
	DROP INDEX holding_eligibility__ix__asset_id;

    CREATE TABLE holding (
		holding_id bigserial NOT NULL,
        account_id int8 NOT NULL,
		account_hash text NOT NULL,
		asset_id int8 NOT NULL,
		asset_hash text NOT NULL,
		balance numeric NULL,
		is_primary_eligible bool NULL,
		is_secondary_eligible bool NULL,
		kyc_controller_address text NULL,

		CONSTRAINT holding__pk PRIMARY KEY (holding_id),
		CONSTRAINT holding__fk__account FOREIGN KEY (account_id) REFERENCES account(account_id),
		CONSTRAINT holding__fk__asset FOREIGN KEY (asset_id) REFERENCES asset(asset_id),
        CONSTRAINT holding__uk__account_id__asset_id UNIQUE (account_id, asset_id)
	);

    INSERT INTO holding (holding_id, asset_id, asset_hash, account_id, account_hash, balance,
        is_primary_eligible, is_secondary_eligible, kyc_controller_address)
    SELECT holding_eligibility_id, asset_id, asset_hash, account_id, account_hash, balance,
        is_primary_eligible, is_secondary_eligible, kyc_controller_address
    FROM holding_eligibility;

    CREATE INDEX holding__ix__account_id ON holding (account_id);
	CREATE INDEX holding__ix__asset_id ON holding (asset_id);

    DROP TABLE holding_eligibility;

    PERFORM setval('holding_holding_id_seq', (SELECT MAX(holding_id) FROM holding) + 1);

    INSERT INTO database_version (version_number, description)
    VALUES (13, 'Rename constraints, add NOT NULL constraints and rename holding and tx tables');
END
$$;
