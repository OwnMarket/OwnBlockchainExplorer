////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class BlockchainEvent
    {
        public long BlockchainEventId { get; set; }
        public string EventType { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Fee { get; set; }
        public long BlockId { get; set; }
        public long? TxId { get; set; }
        public long? EquivocationId { get; set; }
        public long? AddressId { get; set; }
        public long? AssetId { get; set; }
        public long? AccountId { get; set; }
        public long? TxActionId { get; set; }
        public Guid? GroupingId { get; set; }
        
        public virtual Block Block { get; set; }
        public virtual Tx Tx { get; set; }
        public virtual Equivocation Equivocation { get; set; }
        public virtual Address Address { get; set; }
        public virtual Asset Asset { get; set; }
        public virtual Account Account { get; set; }
        public virtual TxAction TxAction { get; set; }

        public BlockchainEvent()
        {
            
        }
    }
}