using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class ConfigurationDto
    {
        public int ConfigurationBlockDelta { get; set; }
        public List<ValidatorDto> Validators { get; set; }
        public List<string> ValidatorsBlacklist { get; set; }
        public List<string> DormantValidators { get; set; }
        public int ValidatorDepositLockTime { get; set; }
        public int ValidatorBlacklistTime { get; set; }
        public int MaxTxCountPerBlock { get; set; }
    }
}
