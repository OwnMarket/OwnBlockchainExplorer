using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class StakingRewardDto
    {
        public string StakerAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
