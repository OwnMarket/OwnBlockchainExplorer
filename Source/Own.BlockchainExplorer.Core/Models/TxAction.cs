////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class TxAction
    {
        public long TxActionId { get; set; }
        public int ActionNumber { get; set; }
        public string ActionType { get; set; }
        public string ActionData { get; set; }
        
        public virtual ICollection<BlockchainEvent> BlockchainEventsByTxActionId { get; set; }

        public TxAction()
        {
            this.BlockchainEventsByTxActionId = new HashSet<BlockchainEvent>();
        }
    }
}