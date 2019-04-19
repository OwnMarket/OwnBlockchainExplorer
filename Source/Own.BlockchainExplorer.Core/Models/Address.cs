////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Address
    {
        public long AddressId { get; set; }
        public string BlockchainAddress { get; set; }
        public long Nonce { get; set; }
        public decimal StakedBalance { get; set; }
        public decimal DepositBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        
        public virtual ICollection<BlockchainEvent> BlockchainEventsByAddressId { get; set; }

        public Address()
        {
            this.BlockchainEventsByAddressId = new HashSet<BlockchainEvent>();
        }
    }
}