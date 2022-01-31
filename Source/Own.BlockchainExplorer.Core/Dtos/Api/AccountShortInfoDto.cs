namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AccountShortInfoDto
    {
        public string Hash { get; set; }
        public long? AssetsCount { get; set; }
        public long? TransfersCount { get; set; }
        public string ControllerAddress { get; set; }
    }
}