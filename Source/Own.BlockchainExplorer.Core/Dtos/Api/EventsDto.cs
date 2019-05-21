using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class EventsDto
    {
        public List<ActionDto> Actions { get; set; }
        public List<ValidatorRewardDto> ValidatorRewards { get; set; }
        public List<StakingRewardDto> StakingRewards { get; set; }
        public List<DepositDto> TakenDeposits { get; set; }
        public List<DepositDto> GivenDeposits { get; set; }
    }
}
