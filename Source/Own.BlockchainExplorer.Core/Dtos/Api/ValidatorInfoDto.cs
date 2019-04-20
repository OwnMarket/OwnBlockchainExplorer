using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ValidatorInfoDto
    {
        public string BlockchainAddress { get; set; }
        public string NetworkAddress { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public bool IsActive { get; set; }

        public List<StakeDto> Stakes { get; set; }
    }

    public class ValidatorInfoShortDto
    {
        public string BlockchainAddress { get; set; }
        public bool IsActive { get; set; }
    }
}
