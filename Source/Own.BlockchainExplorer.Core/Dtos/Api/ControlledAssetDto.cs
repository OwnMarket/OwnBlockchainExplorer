using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ControlledAssetDto
    {
        public string Hash { get; set; }
        public string AssetCode { get; set; }
        public bool IsActive { get; set; }
    }
}
