using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AddressInfoDto
    {
        public string BlockchainAddress { get; set; }
        public long Nonce { get; set; }
        public ChxBalanceInfoDto ChxBalanceInfo { get; set; }

        public static AddressInfoDto FromDomainModel(Address model)
        {
            return new AddressInfoDto
            {
                BlockchainAddress = model.BlockchainAddress,
                Nonce = model.Nonce,
                ChxBalanceInfo = new ChxBalanceInfoDto
                {
                    ValidatorDeposit = model.DepositBalance,
                    DelegatedStakes = model.StakedBalance,
                    AvailableBalance = model.AvailableBalance
                }
            };
        }

        public void ApplyTo(Address model)
        {
            model.BlockchainAddress = BlockchainAddress;
            model.Nonce = Nonce;
            model.DepositBalance = ChxBalanceInfo.ValidatorDeposit;
            model.StakedBalance = ChxBalanceInfo.DelegatedStakes;
            model.AvailableBalance = ChxBalanceInfo.AvailableBalance;
        }
    }

    public class ControllerAddressDto
    {
        public string BlockchainAddress { get; set; }
    }
}
