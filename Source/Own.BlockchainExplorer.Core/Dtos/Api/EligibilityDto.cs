namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class EligibilityDto
    {
        public string AssetHash { get; set; }
        public string AccountHash { get; set; }
        public bool? IsPrimaryEligible { get; set; }
        public bool? IsSecondaryEligible { get; set; }
        public string KycControllerAddress { get; set; }
    }
}
