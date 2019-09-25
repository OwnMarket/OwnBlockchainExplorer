using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AddressSummaryDto
    {
        public List<AddressInfoSlimDto> Addresses { get; set; }
        public int AddressCount { get; set; }
    }

    public class AddressInfoSlimDto
    {
        public string BlockchainAddress { get; set; }
        public decimal TotalBalance { get; set; }
    }
}
