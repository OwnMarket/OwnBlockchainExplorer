namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class ValidatorDto
    {
        public string ValidatorAddress { get; set; }
        public string NetworkAddress { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public decimal TotalStake { get; set; }
    }
}
