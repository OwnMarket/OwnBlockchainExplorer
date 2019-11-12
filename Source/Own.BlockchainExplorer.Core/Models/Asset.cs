////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Asset
    {
        public long AssetId { get; set; }
        public string Hash { get; set; }
        public string AssetCode { get; set; }
        public bool IsEligibilityRequired { get; set; }
        public string ControllerAddress { get; set; }
        
        public virtual ICollection<BlockchainEvent> BlockchainEventsByAssetId { get; set; }
        public virtual ICollection<Holding> HoldingsByAssetId { get; set; }

        public Asset()
        {
            this.BlockchainEventsByAssetId = new HashSet<BlockchainEvent>();
            this.HoldingsByAssetId = new HashSet<Holding>();
        }
    }
}