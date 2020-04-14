using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class BlockConfigurationDto
    {
        public int ConfigurationBlockDelta { get; set; }
        public List<ValidatorSnapshotDto> Validators { get; set; }
        public List<string> ValidatorsBlacklist { get; set; }
        public List<string> DormantValidators { get; set; }
        public int ValidatorDepositLockTime { get; set; }
        public int ValidatorBlacklistTime { get; set; }
        public int MaxTxCountPerBlock { get; set; }
    }

    public class ValidatorSnapshotDto
    {
        public string ValidatorAddress { get; set; }
        public string NetworkAddress { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public decimal TotalStake { get; set; }
    }
}
