using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class TxDto
    {
        public string TxHash { get; set; }
        public string SenderAddress { get; set; }
        public long Nonce { get; set; }
        public decimal ActionFee { get; set; }
        public List<ActionDto> Actions { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public short? FailedActionNumber { get; set; }
        public long? BlockNumber { get; set; }
    }
}
