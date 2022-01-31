using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class BridgeTransferInfoDto
    {
        public TransferType TransferTypeCode { get; set; }
        public BlockchainCode BlockchainCode { get; set; }
        public decimal Amount { get; set; }
    }
}