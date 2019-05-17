namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class TransferAssetData
    {
        public string FromAccountHash { get; set; }
        public string ToAccountHash { get; set; }
        public string AssetHash { get; set; }
        public decimal Amount { get; set; }
    }
}
