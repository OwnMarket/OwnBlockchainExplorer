namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ValidatorStatsDto
    {
        public string BlockchainAddress { get; set; }
        public string NetworkAddress { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public decimal TotalStake { get; set; }
        public decimal Deposit { get; set; }
        public int BlocksProposed { get; set; }
        public int TxsProposed { get; set; }
        public decimal RewardsCollected { get; set; }
        public decimal RewardsDistributed { get; set; }
    }
}
