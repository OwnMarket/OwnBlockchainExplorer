////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Block
    {
        public long BlockId { get; set; }
        public long BlockNumber { get; set; }
        public string Hash { get; set; }
        public long PreviousBlockId { get; set; }
        public string PreviousBlockHash { get; set; }
        public long ConfigurationBlockNumber { get; set; }
        public long Timestamp { get; set; }
        public long ValidatorId { get; set; }
        public string TxSetRoot { get; set; }
        public string TxResultSetRoot { get; set; }
        public string EquivocationProofsRoot { get; set; }
        public string EquivocationProofResultsRoot { get; set; }
        public string StateRoot { get; set; }
        public string StakingRewardsRoot { get; set; }
        public string ConfigurationRoot { get; set; }
        public string Configuration { get; set; }
        public int? ConsensusRound { get; set; }
        public string Signatures { get; set; }
        
        public virtual Block PreviousBlock { get; set; }
        public virtual Validator Validator { get; set; }
        public virtual ICollection<Block> BlocksByPreviousBlockId { get; set; }
        public virtual ICollection<BlockchainEvent> BlockchainEventsByBlockId { get; set; }
        public virtual ICollection<Equivocation> EquivocationsByBlockId { get; set; }

        public Block()
        {
            this.BlocksByPreviousBlockId = new HashSet<Block>();
            this.BlockchainEventsByBlockId = new HashSet<BlockchainEvent>();
            this.EquivocationsByBlockId = new HashSet<Equivocation>();
        }
    }
}