using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AssetShortInfoDto
    {
        public string Hash { get; set; }
        public string AssetCode { get; set; }
        public decimal TotalSupply { get; set; }
        public long? HoldersCount { get; set; }
        public long? TransfersCount { get; set; }
        public string ControllerAddress { get; set; }
        public List<BridgeTransferStatsInfoDto> BridgeTransferStats { get; set; }
    }
}