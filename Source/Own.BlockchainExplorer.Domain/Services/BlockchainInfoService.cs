using System.Linq;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class BlockchainInfoService : DataService, IBlockchainInfoService
    {
        public BlockchainInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<AddressInfoDto> GetAddressInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var addressInfoDto = NewRepository<Address>(uow)
                    .GetAs(
                        a => a.BlockchainAddress == blockchainAddress,
                        a =>
                            new AddressInfoDto
                            {
                                BlockchainAddress = a.BlockchainAddress,
                                Nonce = a.Nonce,
                                ChxBalanceInfo =
                                    new ChxBalanceInfoDto
                                    {
                                        TotalBalance = a.ChxBalance
                                    }
                            })
                    .SingleOrDefault();

                return addressInfoDto == null
                    ? Result.Failure<AddressInfoDto>("Address not found.")
                    : Result.Success(addressInfoDto);
            }
        }
    }
}
