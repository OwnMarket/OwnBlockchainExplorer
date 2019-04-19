////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class HoldingEligibility
    {
        public long HoldingEligibilityId { get; set; }
        public long AssetId { get; set; }
        public string AssetHash { get; set; }
        public long AccountId { get; set; }
        public string AccountHash { get; set; }
        public decimal? Balance { get; set; }
        public bool? IsPrimaryEligible { get; set; }
        public bool? IsSecondaryEligible { get; set; }
        public string KycControllerAddress { get; set; }
        
        public virtual Asset Asset { get; set; }
        public virtual Account Account { get; set; }

        public HoldingEligibility()
        {
            
        }
    }
}