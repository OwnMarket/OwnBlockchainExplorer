////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Equivocation
    {
        public long EquivocationId { get; set; }
        public string EquivocationProofHash { get; set; }
        public long BlockId { get; set; }
        public long BlockNumber { get; set; }
        public int ConsensusRound { get; set; }
        public int ConsensusStep { get; set; }
        public string EquivocationValue1 { get; set; }
        public string EquivocationValue2 { get; set; }
        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
        
        public virtual Block Block { get; set; }
        public virtual ICollection<BlockchainEvent> BlockchainEventsByEquivocationId { get; set; }

        public Equivocation()
        {
            this.BlockchainEventsByEquivocationId = new HashSet<BlockchainEvent>();
        }
    }
}