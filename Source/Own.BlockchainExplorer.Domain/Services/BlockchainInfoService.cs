using System.Linq;
using System;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System.Collections.Generic;

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
                                        AvailableBalance = a.AvailableBalance
                                    }
                            })
                    .SingleOrDefault();

                return addressInfoDto == null
                    ? Result.Failure<AddressInfoDto>("Address not found.")
                    : Result.Success(addressInfoDto);
            }
        }

        public Result<BlockInfoDto> GetBlockInfo(int blockNumber)
        {
            throw new NotImplementedException();
        }

        public Result<TxInfoDto> GetTxInfo(string txHash)
        {
            throw new NotImplementedException();
        }

        public Result<EquivocationInfoDto> GetEquivocationInfo(string EquivocationProofHash)
        {
            throw new NotImplementedException();
        }

        public Result<AccountInfoDto> GetAccountInfo(string accountHash)
        {
            throw new NotImplementedException();
        }

        public Result<AssetInfoDto> GetAssetInfo(string assetHash)
        {
            throw new NotImplementedException();
        }

        public Result<ValidatorInfoDto> GetValidatorInfo(string blockchainAddress)
        {
            throw new NotImplementedException();
        }

        public Result<IEnumerable<TxInfoShortDto>> GetTxs(int limit, int page)
        {
            throw new NotImplementedException();
        }

        public Result<IEnumerable<BlockInfoShortDto>> GetBlocks(int limit, int page)
        {
            throw new NotImplementedException();
        }
        public Result<IEnumerable<ValidatorInfoShortDto>> GetValidators()
        {
            throw new NotImplementedException();
        }
    }
}
