using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class HoldingDto
    {
        public string AccountHash { get; set; }
        public string AssetHash { get; set; }
        public decimal Balance { get; set; }
    }
}
