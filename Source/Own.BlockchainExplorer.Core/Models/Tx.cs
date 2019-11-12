////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Tx
    {
        public long TxId { get; set; }
        public string Hash { get; set; }
        public long Nonce { get; set; }
        public long Timestamp { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public decimal ActionFee { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public short? FailedActionNumber { get; set; }
        
        public virtual ICollection<BlockchainEvent> BlockchainEventsByTxId { get; set; }

        public Tx()
        {
            this.BlockchainEventsByTxId = new HashSet<BlockchainEvent>();
        }
    }
}