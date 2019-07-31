////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Validator
    {
        public long ValidatorId { get; set; }
        public string BlockchainAddress { get; set; }
        public string NetworkAddress { get; set; }
        public string GeoLocation { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        
        public virtual ICollection<Block> BlocksByValidatorId { get; set; }

        public Validator()
        {
            this.BlocksByValidatorId = new HashSet<Block>();
        }
    }
}