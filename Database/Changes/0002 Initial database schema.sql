\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN
	CREATE TABLE account (
		account_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		hash text NOT NULL,
		controller_address text NOT NULL,
		
		CONSTRAINT account_pk PRIMARY KEY (account_id)
	);

	-- Drop table

	-- DROP TABLE "action"

	CREATE TABLE "tx_action" (
		tx_action_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		action_number int4 NOT NULL,
		action_type text NOT NULL,
		action_data text NULL,
		
		CONSTRAINT tx_action_pk PRIMARY KEY (tx_action_id)
	);

	-- Drop table

	-- DROP TABLE address

	CREATE TABLE address (
		address_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		blockchain_address text NOT NULL,
		nonce int8 NOT NULL,
		staked_balance numeric NOT NULL,
		deposit_balance numeric NOT NULL,
		available_balance numeric NOT NULL,

		CONSTRAINT address_pk PRIMARY KEY (address_id)
	);

	-- Drop table

	-- DROP TABLE asset

	CREATE TABLE asset (
		asset_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		hash text NOT NULL,
		asset_code text NULL,
		is_eligibility_required bool NULL,
		controller_address text NOT NULL,
		
		CONSTRAINT asset_pk PRIMARY KEY (asset_id)
	);
	
	-- Drop table

	-- DROP TABLE "validator"

	CREATE TABLE "validator" (
		validator_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		blockchain_address text NOT NULL,
		network_address text NULL,
		shared_reward_percent numeric NOT NULL,
		is_active bool NOT NULL,
		
		CONSTRAINT validator_pk PRIMARY KEY (validator_id)
	);
	
	-- Drop table

	-- DROP TABLE "transaction"

	CREATE TABLE "transaction" (
		transaction_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		hash text NOT NULL,
		nonce int8 NOT NULL,
		"timestamp" timestamp NOT NULL,
		expiration_time timestamp NULL,
		action_fee numeric NOT NULL,
		status text NOT NULL,
		error_message text NULL,
		failed_action_number int2 NULL,
		
		CONSTRAINT transaction_pk PRIMARY KEY (transaction_id)
	);

	-- Drop table

	-- DROP TABLE block

	CREATE TABLE block (
		block_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		block_number int8 NOT NULL,
		hash text NOT NULL,
		previous_block_id int8 NOT NULL,
		previous_block_hash text NOT NULL,
		configuration_block_number int8 NOT NULL,
		"timestamp" int8 NOT NULL,
		validator_id int8 NOT NULL,
		tx_set_root text NULL,
		tx_result_set_root text NULL,
		equivocation_proofs_root text NULL,
		equivocation_proof_results_root text NULL,
		state_root text NULL,
		staking_rewards_root text NULL,
		configuration_root text NULL,
		"configuration" json NULL,
		consensus_round int4 NULL,
		signatures text NOT NULL,
		
		CONSTRAINT block_pk PRIMARY KEY (block_id),
		CONSTRAINT block_block_fk FOREIGN KEY (previous_block_id) REFERENCES block(block_id),
		CONSTRAINT block_validator_fk FOREIGN KEY (validator_id) REFERENCES validator(validator_id)
	);

	-- Drop table

	-- DROP TABLE equivocation

	CREATE TABLE equivocation (
		equivocation_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		equivocation_proof_hash text NOT NULL,
		block_id int8 NOT NULL,
		block_number int8 NOT NULL,
		consensus_round int4 NOT NULL,
		consensus_step int4 NOT NULL,
		equivocation_value_1 text NULL,
		equivocation_value_2 text NULL,
		signature_1 text NULL,
		signature_2 text NULL,
		
		CONSTRAINT equivocation_pk PRIMARY KEY (equivocation_id),
		CONSTRAINT equivocation_block_fk FOREIGN KEY (block_id) REFERENCES block(block_id)
	);

	-- Drop table

	-- DROP TABLE "event"

	CREATE TABLE "blockchain_event" (
		blockchain_event_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		event_type text NOT NULL,
		amount numeric NULL,
		fee text NULL,
		block_id int8 NOT NULL,
		transaction_id int8 NULL,
		equivocation_id int8 NULL,
		address_id int8 NULL,
		asset_id int8 NULL,
		account_id int8 NULL,
		tx_action_id int8 NULL,
		
		CONSTRAINT blockchain_event_pk PRIMARY KEY (blockchain_event_id),
		CONSTRAINT blockchain_event_account_fk FOREIGN KEY (account_id) REFERENCES account(account_id),
		CONSTRAINT blockchain_event_tx_action_fk FOREIGN KEY (tx_action_id) REFERENCES tx_action(tx_action_id),
		CONSTRAINT blockchain_event_address_fk FOREIGN KEY (address_id) REFERENCES address(address_id),
		CONSTRAINT blockchain_event_asset_fk FOREIGN KEY (asset_id) REFERENCES asset(asset_id),
		CONSTRAINT blockchain_event_block_fk FOREIGN KEY (block_id) REFERENCES block(block_id),
		CONSTRAINT blockchain_event_equivocation_fk FOREIGN KEY (equivocation_id) REFERENCES equivocation(equivocation_id),
		CONSTRAINT blockchain_event_transaction_fk FOREIGN KEY (transaction_id) REFERENCES transaction(transaction_id)
	);

	-- Drop table

	-- DROP TABLE holding_eligibility

	CREATE TABLE holding_eligibility (
		holding_eligibility_id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
		asset_id int8 NOT NULL,
		asset_hash text NOT NULL,
		account_id int8 NOT NULL,
		account_hash text NOT NULL,
		balance numeric NULL,
		is_primary_eligible bool NULL,
		is_secondary_eligible bool NULL,
		kyc_controller_address text NULL,

		CONSTRAINT eligibility_pk PRIMARY KEY (holding_eligibility_id),
		CONSTRAINT eligibility_account_fk FOREIGN KEY (account_id) REFERENCES account(account_id),
		CONSTRAINT eligibility_asset_fk FOREIGN KEY (asset_id) REFERENCES asset(asset_id)
	);

    INSERT INTO database_version (version_number, description)
    VALUES (2, 'Initial database schema');
END
$$;
