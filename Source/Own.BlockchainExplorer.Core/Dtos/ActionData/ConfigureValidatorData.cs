namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class ConfigureValidatorData
    {
        public string NetworkAddress { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public bool IsEnabled { get; set; }
    }
}
