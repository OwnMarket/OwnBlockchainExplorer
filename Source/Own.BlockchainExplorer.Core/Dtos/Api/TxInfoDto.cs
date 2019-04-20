using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class TxInfoDto
    {
        public string Hash { get; set; }
        public long BlockNumber { get; set; }
        public string SenderAddress { get; set; }
        public long Nonce { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public decimal ActionFee { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public short? FailedActionNumber { get; set; }

        public List<ActionDto> Actions { get; set; }
    }

    public class TxInfoShortDto
    {
        public string Hash { get; set; }
        public long BlockNumber { get; set; }
        public string SenderAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public int NumberOfActions { get; set; }
    }
}
