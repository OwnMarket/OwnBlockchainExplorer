////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Transaction
    {
        public long TransactionId { get; set; }
        public string Hash { get; set; }
        public long Nonce { get; set; }
        public long Timestamp { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public decimal ActionFee { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public short? FailedActionNumber { get; set; }
        
        public virtual ICollection<BlockchainEvent> BlockchainEventsByTransactionId { get; set; }

        public Transaction()
        {
            this.BlockchainEventsByTransactionId = new HashSet<BlockchainEvent>();
        }
    }
}