using System;
using Own.BlockchainExplorer.Core.Dtos.ActionData;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AssetTransferInfoDto : TransferAssetData
    {
        public string Hash { get; set; }
        public DateTime Date { get; set; }
    }
}