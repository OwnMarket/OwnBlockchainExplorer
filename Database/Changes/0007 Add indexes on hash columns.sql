\set ON_ERROR_STOP on

SET search_path TO own, public;

DO $$
BEGIN

	DROP INDEX IF EXISTS account__ix__hash;
	CREATE INDEX account__ix__hash ON account (hash);
	
	DROP INDEX IF EXISTS address__ix__blockchain_address;
	CREATE INDEX address__ix__blockchain_address ON address (blockchain_address);
	
	DROP INDEX IF EXISTS asset__ix__hash;
	CREATE INDEX asset__ix__hash ON asset (hash);
	
	DROP INDEX IF EXISTS block__ix__block_number;
	CREATE INDEX block__ix__block_number ON block (block_number);
	
	DROP INDEX IF EXISTS equivocation__ix__equivocation_proof_hash;
	CREATE INDEX equivocation__ix__equivocation_proof_hash ON equivocation (equivocation_proof_hash);
	
	DROP INDEX IF EXISTS transaction__ix__hash;
	CREATE INDEX transaction__ix__hash ON "transaction" (hash);
	
	DROP INDEX IF EXISTS validator__ix__blockchain_address;
	CREATE INDEX validator__ix__blockchain_address ON validator (blockchain_address);

    INSERT INTO database_version (version_number, description)
    VALUES (7, 'Add indexes on hash columns');
END
$$;
