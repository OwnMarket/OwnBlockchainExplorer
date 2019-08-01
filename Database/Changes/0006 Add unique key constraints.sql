\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

	ALTER TABLE account ADD CONSTRAINT account__uk__hash UNIQUE (hash);
	ALTER TABLE address ADD CONSTRAINT address__uk__address UNIQUE (blockchain_address);
	ALTER TABLE asset ADD CONSTRAINT asset__uk__hash UNIQUE (hash);
    ALTER TABLE asset ADD CONSTRAINT asset__uk__asset_code UNIQUE (asset_code);
    ALTER TABLE validator ADD CONSTRAINT validator__uk_address UNIQUE (blockchain_address);
    ALTER TABLE equivocation ADD CONSTRAINT equivocation__uk__equivocation_proof_hash UNIQUE (equivocation_proof_hash);
    ALTER TABLE holding_eligibility ADD CONSTRAINT eligibility__uk__account__asset UNIQUE (account_id, asset_id);

    INSERT INTO database_version (version_number, description)
    VALUES (6, 'Add unique key constraints');
END
$$;
