namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class SetAccountEligibilityData
    {
        public string AccountHash { get; set; }
        public string AssetHash { get; set; }
        public bool IsPrimaryEligible { get; set; }
        public bool IsSecondaryEligible { get; set; }
    }
}
