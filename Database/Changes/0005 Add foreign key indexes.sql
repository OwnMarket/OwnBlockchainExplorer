\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

	-- block
	DROP INDEX IF EXISTS block__ix__previous_block_id;
	CREATE INDEX block__ix__previous_block_id ON block (previous_block_id);

	DROP INDEX IF EXISTS block__ix__validator_id;
	CREATE INDEX block__ix__validator_id ON block (validator_id);

	-- equivocation
	DROP INDEX IF EXISTS equivocation__ix__block_id;
	CREATE INDEX equivocation__ix__block_id ON equivocation (block_id);


	-- blockchain_event
	DROP INDEX IF EXISTS blockchain_event__ix__account_id;
	CREATE INDEX blockchain_event__ix__account_id ON blockchain_event (account_id);

	DROP INDEX IF EXISTS blockchain_event__ix__tx_action_id;
	CREATE INDEX blockchain_event__ix__tx_action_id ON blockchain_event (tx_action_id);

	DROP INDEX IF EXISTS blockchain_event__ix__address_id;
	CREATE INDEX blockchain_event__ix__address_id ON blockchain_event (address_id);

	DROP INDEX IF EXISTS blockchain_event__ix__asset_id;
	CREATE INDEX blockchain_event__ix__asset_id ON blockchain_event (asset_id);

	DROP INDEX IF EXISTS blockchain_event__ix__block_id;
	CREATE INDEX blockchain_event__ix__block_id ON blockchain_event (block_id);

	DROP INDEX IF EXISTS blockchain_event__ix__equivocation_id;
	CREATE INDEX blockchain_event__ix__equivocation_id ON blockchain_event (equivocation_id);

	DROP INDEX IF EXISTS blockchain_event__ix__transaction_id;
	CREATE INDEX blockchain_event__ix__transaction_id ON blockchain_event (transaction_id);


	-- holding_eligibility
	DROP INDEX IF EXISTS holding_eligibility__ix__account_id;
	CREATE INDEX holding_eligibility__ix__account_id ON holding_eligibility (account_id);

	DROP INDEX IF EXISTS holding_eligibility__ix__asset_id;
	CREATE INDEX holding_eligibility__ix__asset_id ON holding_eligibility (asset_id);


    INSERT INTO database_version (version_number, description)
    VALUES (5, 'Add foreign key indexes');
END
$$;
