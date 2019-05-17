namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class CreateAssetEmissionData
    {
        public string EmissionAccountHash { get; set; }
        public string AssetHash { get; set; }
        public decimal Amount { get; set; }
    }
}
