using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AccountInfoDto
    {
        public string Hash { get; set; }
        public string ControllerAddress { get; set; }

        public List<HoldingDto> Holdings { get; set; }
        public List<EligibilityDto> Eligibilities { get; set; }
        public List<ControllerAddressDto> ControllerAddresses { get; set; }
    }
}
