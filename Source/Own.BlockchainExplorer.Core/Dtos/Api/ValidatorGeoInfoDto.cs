using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ValidatorGeoInfoDto
    {
        public string NetworkAddress { get; set; }
        public GeoLocationDto Location { get; set; }
    }
}
