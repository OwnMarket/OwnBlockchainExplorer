using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class BridgeTransferStatsInfoDto
    {
        public BlockchainCode BlockchainCode { get; set; }

        public string ContractAddress { get; set; }

        public long TransfersCount { get; set; }

        public decimal CirculatingSupply { get; set; }
    }
}