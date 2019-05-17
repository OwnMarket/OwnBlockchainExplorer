using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class AddKycProviderData
    {
        public string AssetHash { get; set; }
        public string ProviderAddress { get; set; }
    }
}
