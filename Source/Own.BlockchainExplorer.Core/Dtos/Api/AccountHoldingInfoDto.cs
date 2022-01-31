namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AccountHoldingInfoDto
    {
        public string AssetHash { get; set; }
        public string AssetCode { get; set; }
        public decimal? Balance { get; set; }
    }
}