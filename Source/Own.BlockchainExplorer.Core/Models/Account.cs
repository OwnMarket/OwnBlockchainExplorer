////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Account
    {
        public long AccountId { get; set; }
        public string Hash { get; set; }
        public string ControllerAddress { get; set; }
        
        public virtual ICollection<BlockchainEvent> BlockchainEventsByAccountId { get; set; }
        public virtual ICollection<Holding> HoldingsByAccountId { get; set; }

        public Account()
        {
            this.BlockchainEventsByAccountId = new HashSet<BlockchainEvent>();
            this.HoldingsByAccountId = new HashSet<Holding>();
        }
    }
}