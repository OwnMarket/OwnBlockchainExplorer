////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Models;

namespace Own.BlockchainExplorer.Infrastructure.Data.EF
{
    public partial class OwnDb : DbContext
    {
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<Block> Blocks { get; set; }
        public virtual DbSet<BlockchainEvent> BlockchainEvents { get; set; }
        public virtual DbSet<Equivocation> Equivocations { get; set; }
        public virtual DbSet<Holding> Holdings { get; set; }
        public virtual DbSet<Tx> Txs { get; set; }
        public virtual DbSet<TxAction> TxActions { get; set; }
        public virtual DbSet<Validator> Validators { get; set; }

        private void ConfigureEntities(ModelBuilder modelBuilder)
        {
            // Account
            var account = modelBuilder.Entity<Account>()
                .ToTable("account", "own");
            account.HasKey(c => c.AccountId);
            account.Property(e => e.AccountId)
                .HasColumnName("account_id")
                .IsRequired();
            account.Property(e => e.Hash)
                .HasColumnName("hash")
                .IsRequired();
            account.Property(e => e.ControllerAddress)
                .HasColumnName("controller_address")
                .IsRequired();
            
            // Address
            var address = modelBuilder.Entity<Address>()
                .ToTable("address", "own");
            address.HasKey(c => c.AddressId);
            address.Property(e => e.AddressId)
                .HasColumnName("address_id")
                .IsRequired();
            address.Property(e => e.BlockchainAddress)
                .HasColumnName("blockchain_address")
                .IsRequired();
            address.Property(e => e.Nonce)
                .HasColumnName("nonce")
                .IsRequired();
            address.Property(e => e.StakedBalance)
                .HasColumnName("staked_balance")
                .IsRequired();
            address.Property(e => e.DepositBalance)
                .HasColumnName("deposit_balance")
                .IsRequired();
            address.Property(e => e.AvailableBalance)
                .HasColumnName("available_balance")
                .IsRequired();
            
            // Asset
            var asset = modelBuilder.Entity<Asset>()
                .ToTable("asset", "own");
            asset.HasKey(c => c.AssetId);
            asset.Property(e => e.AssetId)
                .HasColumnName("asset_id")
                .IsRequired();
            asset.Property(e => e.Hash)
                .HasColumnName("hash")
                .IsRequired();
            asset.Property(e => e.AssetCode)
                .HasColumnName("asset_code");
            asset.Property(e => e.IsEligibilityRequired)
                .HasColumnName("is_eligibility_required")
                .IsRequired();
            asset.Property(e => e.ControllerAddress)
                .HasColumnName("controller_address")
                .IsRequired();
            
            // Block
            var block = modelBuilder.Entity<Block>()
                .ToTable("block", "own");
            block.HasKey(c => c.BlockId);
            block.Property(e => e.BlockId)
                .HasColumnName("block_id")
                .IsRequired();
            block.Property(e => e.BlockNumber)
                .HasColumnName("block_number")
                .IsRequired();
            block.Property(e => e.Hash)
                .HasColumnName("hash")
                .IsRequired();
            block.Property(e => e.PreviousBlockId)
                .HasColumnName("previous_block_id");
            block.Property(e => e.PreviousBlockHash)
                .HasColumnName("previous_block_hash")
                .IsRequired();
            block.Property(e => e.ConfigurationBlockNumber)
                .HasColumnName("configuration_block_number")
                .IsRequired();
            block.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();
            block.Property(e => e.ValidatorId)
                .HasColumnName("validator_id")
                .IsRequired();
            block.Property(e => e.TxSetRoot)
                .HasColumnName("tx_set_root");
            block.Property(e => e.TxResultSetRoot)
                .HasColumnName("tx_result_set_root");
            block.Property(e => e.EquivocationProofsRoot)
                .HasColumnName("equivocation_proofs_root");
            block.Property(e => e.EquivocationProofResultsRoot)
                .HasColumnName("equivocation_proof_results_root");
            block.Property(e => e.StateRoot)
                .HasColumnName("state_root");
            block.Property(e => e.StakingRewardsRoot)
                .HasColumnName("staking_rewards_root");
            block.Property(e => e.ConfigurationRoot)
                .HasColumnName("configuration_root");
            block.Property(e => e.Configuration)
                .HasColumnName("configuration")
                .HasColumnType("json");
            block.Property(e => e.ConsensusRound)
                .HasColumnName("consensus_round")
                .IsRequired();
            block.Property(e => e.Signatures)
                .HasColumnName("signatures")
                .IsRequired();
            block.HasOne(e => e.PreviousBlock)
                .WithMany(e => e.BlocksByPreviousBlockId)
                .HasForeignKey(e => e.PreviousBlockId);
            block.HasOne(e => e.Validator)
                .WithMany(e => e.BlocksByValidatorId)
                .IsRequired()
                .HasForeignKey(e => e.ValidatorId);
            
            // BlockchainEvent
            var blockchainEvent = modelBuilder.Entity<BlockchainEvent>()
                .ToTable("blockchain_event", "own");
            blockchainEvent.HasKey(c => c.BlockchainEventId);
            blockchainEvent.Property(e => e.BlockchainEventId)
                .HasColumnName("blockchain_event_id")
                .IsRequired();
            blockchainEvent.Property(e => e.EventType)
                .HasColumnName("event_type")
                .IsRequired();
            blockchainEvent.Property(e => e.Amount)
                .HasColumnName("amount");
            blockchainEvent.Property(e => e.Fee)
                .HasColumnName("fee");
            blockchainEvent.Property(e => e.BlockId)
                .HasColumnName("block_id")
                .IsRequired();
            blockchainEvent.Property(e => e.TxId)
                .HasColumnName("tx_id");
            blockchainEvent.Property(e => e.EquivocationId)
                .HasColumnName("equivocation_id");
            blockchainEvent.Property(e => e.AddressId)
                .HasColumnName("address_id");
            blockchainEvent.Property(e => e.AssetId)
                .HasColumnName("asset_id");
            blockchainEvent.Property(e => e.AccountId)
                .HasColumnName("account_id");
            blockchainEvent.Property(e => e.TxActionId)
                .HasColumnName("tx_action_id");
            blockchainEvent.HasOne(e => e.Block)
                .WithMany(e => e.BlockchainEventsByBlockId)
                .IsRequired()
                .HasForeignKey(e => e.BlockId);
            blockchainEvent.HasOne(e => e.Tx)
                .WithMany(e => e.BlockchainEventsByTxId)
                .HasForeignKey(e => e.TxId);
            blockchainEvent.HasOne(e => e.Equivocation)
                .WithMany(e => e.BlockchainEventsByEquivocationId)
                .HasForeignKey(e => e.EquivocationId);
            blockchainEvent.HasOne(e => e.Address)
                .WithMany(e => e.BlockchainEventsByAddressId)
                .HasForeignKey(e => e.AddressId);
            blockchainEvent.HasOne(e => e.Asset)
                .WithMany(e => e.BlockchainEventsByAssetId)
                .HasForeignKey(e => e.AssetId);
            blockchainEvent.HasOne(e => e.Account)
                .WithMany(e => e.BlockchainEventsByAccountId)
                .HasForeignKey(e => e.AccountId);
            blockchainEvent.HasOne(e => e.TxAction)
                .WithMany(e => e.BlockchainEventsByTxActionId)
                .HasForeignKey(e => e.TxActionId);
            
            // Equivocation
            var equivocation = modelBuilder.Entity<Equivocation>()
                .ToTable("equivocation", "own");
            equivocation.HasKey(c => c.EquivocationId);
            equivocation.Property(e => e.EquivocationId)
                .HasColumnName("equivocation_id")
                .IsRequired();
            equivocation.Property(e => e.EquivocationProofHash)
                .HasColumnName("equivocation_proof_hash")
                .IsRequired();
            equivocation.Property(e => e.BlockId)
                .HasColumnName("block_id")
                .IsRequired();
            equivocation.Property(e => e.BlockNumber)
                .HasColumnName("block_number")
                .IsRequired();
            equivocation.Property(e => e.ConsensusRound)
                .HasColumnName("consensus_round")
                .IsRequired();
            equivocation.Property(e => e.ConsensusStep)
                .HasColumnName("consensus_step")
                .IsRequired();
            equivocation.Property(e => e.EquivocationValue1)
                .HasColumnName("equivocation_value_1");
            equivocation.Property(e => e.EquivocationValue2)
                .HasColumnName("equivocation_value_2");
            equivocation.Property(e => e.Signature1)
                .HasColumnName("signature_1")
                .IsRequired();
            equivocation.Property(e => e.Signature2)
                .HasColumnName("signature_2")
                .IsRequired();
            equivocation.HasOne(e => e.Block)
                .WithMany(e => e.EquivocationsByBlockId)
                .IsRequired()
                .HasForeignKey(e => e.BlockId);
            
            // Holding
            var holding = modelBuilder.Entity<Holding>()
                .ToTable("holding", "own");
            holding.HasKey(c => c.HoldingId);
            holding.Property(e => e.HoldingId)
                .HasColumnName("holding_id")
                .IsRequired()
                .ValueGeneratedOnAdd();
            holding.Property(e => e.AccountId)
                .HasColumnName("account_id")
                .IsRequired();
            holding.Property(e => e.AccountHash)
                .HasColumnName("account_hash")
                .IsRequired();
            holding.Property(e => e.AssetId)
                .HasColumnName("asset_id")
                .IsRequired();
            holding.Property(e => e.AssetHash)
                .HasColumnName("asset_hash")
                .IsRequired();
            holding.Property(e => e.Balance)
                .HasColumnName("balance");
            holding.Property(e => e.IsPrimaryEligible)
                .HasColumnName("is_primary_eligible");
            holding.Property(e => e.IsSecondaryEligible)
                .HasColumnName("is_secondary_eligible");
            holding.Property(e => e.KycControllerAddress)
                .HasColumnName("kyc_controller_address");
            holding.HasOne(e => e.Account)
                .WithMany(e => e.HoldingsByAccountId)
                .IsRequired()
                .HasForeignKey(e => e.AccountId);
            holding.HasOne(e => e.Asset)
                .WithMany(e => e.HoldingsByAssetId)
                .IsRequired()
                .HasForeignKey(e => e.AssetId);
            
            // Tx
            var tx = modelBuilder.Entity<Tx>()
                .ToTable("tx", "own");
            tx.HasKey(c => c.TxId);
            tx.Property(e => e.TxId)
                .HasColumnName("tx_id")
                .IsRequired();
            tx.Property(e => e.Hash)
                .HasColumnName("hash")
                .IsRequired();
            tx.Property(e => e.Nonce)
                .HasColumnName("nonce")
                .IsRequired();
            tx.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();
            tx.Property(e => e.DateTime)
                .HasColumnName("date_time")
                .IsRequired();
            tx.Property(e => e.ExpirationTime)
                .HasColumnName("expiration_time");
            tx.Property(e => e.ActionFee)
                .HasColumnName("action_fee")
                .IsRequired();
            tx.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired();
            tx.Property(e => e.ErrorMessage)
                .HasColumnName("error_message");
            tx.Property(e => e.FailedActionNumber)
                .HasColumnName("failed_action_number");
            
            // TxAction
            var txAction = modelBuilder.Entity<TxAction>()
                .ToTable("tx_action", "own");
            txAction.HasKey(c => c.TxActionId);
            txAction.Property(e => e.TxActionId)
                .HasColumnName("tx_action_id")
                .IsRequired();
            txAction.Property(e => e.ActionNumber)
                .HasColumnName("action_number")
                .IsRequired();
            txAction.Property(e => e.ActionType)
                .HasColumnName("action_type")
                .IsRequired();
            txAction.Property(e => e.ActionData)
                .HasColumnName("action_data")
                .HasColumnType("json")
                .IsRequired();
            
            // Validator
            var validator = modelBuilder.Entity<Validator>()
                .ToTable("validator", "own");
            validator.HasKey(c => c.ValidatorId);
            validator.Property(e => e.ValidatorId)
                .HasColumnName("validator_id")
                .IsRequired();
            validator.Property(e => e.BlockchainAddress)
                .HasColumnName("blockchain_address")
                .IsRequired();
            validator.Property(e => e.NetworkAddress)
                .HasColumnName("network_address");
            validator.Property(e => e.GeoLocation)
                .HasColumnName("geo_location")
                .HasColumnType("json");
            validator.Property(e => e.SharedRewardPercent)
                .HasColumnName("shared_reward_percent")
                .IsRequired();
            validator.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired();
            validator.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .IsRequired();
        }
    }
}