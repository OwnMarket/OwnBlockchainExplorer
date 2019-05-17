namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class SetAssetEligibilityData
    {
        public string AssetHash { get; set; }
        public bool IsEligibilityRequired { get; set; }
    }
}
