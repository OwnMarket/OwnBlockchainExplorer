using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class DepositDto
    {
        public string BlockchainAddress { get; set; }
        public decimal Amount { get; set; }
        public string EquivocationProofHash { get; set; }
    }
}
