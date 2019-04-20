using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AssetInfoDto
    {
        public string Hash { get; set; }
        public string AssetCode { get; set; }
        public bool? IsEligibilityRequired { get; set; }
        public string ControllerAddress { get; set; }

        public List<HoldingDto> Holdings { get; set; }
        public List<EligibilityDto> Eligibilities { get; set; }
        public List<ControllerAddressDto> ControllerAddresses { get; set; }
    }
}
