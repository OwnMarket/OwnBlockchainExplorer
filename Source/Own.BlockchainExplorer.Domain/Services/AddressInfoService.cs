﻿using System.Linq;
using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System;
using System.Text;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class AddressInfoService : DataService, IAddressInfoService
    {
        private readonly IAddressInfoRepositoryFactory _addressInfoRepositoryFactory;

        public AddressInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IAddressInfoRepositoryFactory addressInfoRepositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _addressInfoRepositoryFactory = addressInfoRepositoryFactory;
        }

        public Result<IEnumerable<ControlledAccountDto>> GetAccountsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    _addressInfoRepositoryFactory.Create(uow).GetAccountsInfo(
                        blockchainAddress,
                        page,
                        limit,
                        isActive));
            }
        }

        public Result<IEnumerable<ControlledAssetDto>> GetAssetsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    _addressInfoRepositoryFactory.Create(uow).GetAssetsInfo(
                        blockchainAddress,
                        page,
                        limit,
                        isActive));
            }
        }

        public Result<StakeSummaryDto> GetDelegatedStakesInfo(
            string blockchainAddress,
            int page,
            int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    _addressInfoRepositoryFactory.Create(uow).GetDelegatedStakesInfo(
                        blockchainAddress,
                        page,
                        limit));
            }
        }

        public Result<StakeSummaryDto> GetReceivedStakesInfo(string blockchainAddress, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    _addressInfoRepositoryFactory.Create(uow).GetReceivedStakesInfo(
                        blockchainAddress,
                        page,
                        limit));
            }
        }

        public Result<EventsSummaryDto> GetEventsInfo(string blockchainAddress, string filter, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    _addressInfoRepositoryFactory.Create(uow).GetEventsInfo(
                        blockchainAddress,
                        filter,
                        page,
                        limit));
            }
        }

        public Result<FileDto> GetEventsCSV(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventDtos = _addressInfoRepositoryFactory
                    .Create(uow)
                    .GetEventsInfo(blockchainAddress, string.Empty, 1, int.MaxValue)
                    .Events;

                var rows = eventDtos
                    .Select(e => e.GetCsvRow())
                    .ToList();
                rows.Insert(0, "Block number,Date/Time,TX,Event info,Amount,Fee");

                var fileContent = string.Join(Environment.NewLine, rows);

                var file = new FileDto
                {
                    Name = $"Events-{blockchainAddress}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.csv",
                    Type = "text/csv",
                    Content = Encoding.UTF8.GetBytes(fileContent)
                };

                return Result.Success(file);
            }
        }

        public Result<AddressInfoDto> GetAddressInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var address = NewRepository<Address>(uow)
                    .Get(a => a.BlockchainAddress == blockchainAddress)
                    .SingleOrDefault();

                if (address is null)
                    return Result.Failure<AddressInfoDto>("Address {0} does not exist.".F(blockchainAddress));

                return Result.Success(AddressInfoDto.FromDomainModel(address));
            }
        }

        public Result<decimal> GetTotalChxBalanceInfo(string blockchainAddress)
        {
            var addressInfoResult = GetAddressInfo(blockchainAddress);
            return addressInfoResult.Failed
                ? Result.Failure<decimal>(addressInfoResult.Alerts)
                : Result.Success(addressInfoResult.Data.ChxBalanceInfo.TotalBalance);
        }

        public Result<decimal> GetAvailableChxBalanceInfo(string blockchainAddress)
        {
            var addressInfoResult = GetAddressInfo(blockchainAddress);
            return addressInfoResult.Failed
                ? Result.Failure<decimal>(addressInfoResult.Alerts)
                : Result.Success(addressInfoResult.Data.ChxBalanceInfo.AvailableBalance);
        }
    }
}
