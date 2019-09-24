using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class StakeDto
    {
        public string StakerAddress { get; set; }
        public string ValidatorAddress { get; set; }
        public decimal Amount { get; set; }
    }

    public class StakeSummaryDto
    {
        public List<StakeDto> Stakes;
        public decimal TotalAmount;
    }
}
