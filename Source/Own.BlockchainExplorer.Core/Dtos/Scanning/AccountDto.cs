using Newtonsoft.Json;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class AccountDto
    {
        [JsonRequired]
        public string AccountHash { get; set; }
        public string ControllerAddress { get; set; }
        public List<AccountHoldingDto> Holdings { get; set; }
    }

    public class AccountHoldingDto
    {
        public string AssetHash { get; set; }
        public decimal Balance { get; set; }
    }
}
