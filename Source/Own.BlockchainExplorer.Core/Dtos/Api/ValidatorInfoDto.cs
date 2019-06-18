using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ValidatorInfoDto
    {
        public string BlockchainAddress { get; set; }
        public string NetworkAddress { get; set; }
        public decimal SharedRewardPercent { get; set; }
        public bool IsActive { get; set; }

        public static ValidatorInfoDto FromDomainModel(Validator validator)
        {
            return new ValidatorInfoDto
            {
                BlockchainAddress = validator.BlockchainAddress,
                NetworkAddress = validator.NetworkAddress,
                SharedRewardPercent = validator.SharedRewardPercent,
                IsActive = validator.IsActive
            };
        }
    }

    public class ValidatorInfoShortDto
    {
        public string BlockchainAddress { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfStakers { get; set; }
        public decimal TotalStake { get; set; }
    }
}
