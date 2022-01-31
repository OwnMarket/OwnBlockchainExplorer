using System;
using Own.BlockchainExplorer.Core.Dtos.ActionData;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AccountTransfersInfoDto : TransferAssetData
    {
        public string Hash { get; set; }
        public DateTime Date { get; set; }
        public string AssetCode { get; set; }
    }
}