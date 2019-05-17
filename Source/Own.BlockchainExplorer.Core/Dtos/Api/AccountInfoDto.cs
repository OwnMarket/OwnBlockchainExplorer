using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AccountInfoDto
    {
        public string Hash { get; set; }
        public string ControllerAddress { get; set; }

        public List<HoldingDto> Holdings { get; set; }
        public List<EligibilityDto> Eligibilities { get; set; }
        public List<ControllerAddressDto> ControllerAddresses { get; set; }

        public static AccountInfoDto FromDomainModel(Account account)
        {
            return new AccountInfoDto
            {
                Hash = account.Hash,
                ControllerAddress = account.ControllerAddress
            };
        }
    }
}
