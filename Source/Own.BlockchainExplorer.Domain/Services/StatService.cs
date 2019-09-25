using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class StatService : DataService, IStatService
    {
        private readonly IBlockchainInfoRepositoryFactory _blockchainInfoRepositoryFactory;
        private readonly IBlockInfoService _blockInfoService;
        private readonly IBlockchainClient _blockchainClient;

        public StatService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IBlockchainInfoRepositoryFactory blockchainInfoRepositoryFactory,
            IBlockInfoService blockInfoService,
            IBlockchainClient blockchainClient)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainInfoRepositoryFactory = blockchainInfoRepositoryFactory;
            _blockInfoService = blockInfoService;
            _blockchainClient = blockchainClient;
        }

        public Result<IEnumerable<KeyValuePair<DateTime, int>>> GetTxPerDay(int numberOfDays)
        {
            using (var uow = NewUnitOfWork())
            {
                var currentDate = DateTime.UtcNow.Date;
                var minDate = currentDate.AddDays(-1 * numberOfDays);

                var result = _blockchainInfoRepositoryFactory.Create(uow)
                    .GetTxPerDay(numberOfDays);

                var tempDate = minDate.AddDays(1);

                while (tempDate <= currentDate)
                {
                    if (!result.Exists(p => p.Key == tempDate))
                        result.Add(new KeyValuePair<DateTime, int>(tempDate, 0));
                    tempDate = tempDate.AddDays(1);
                }

                return Result.Success(result
                    .OrderByDescending(p => p.Key)
                    .AsEnumerable());
            }
        }

        public Result<IEnumerable<ValidatorStatsDto>> GetValidatorStats(int? numberOfDays)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var blockchainInfoRepo = _blockchainInfoRepositoryFactory.Create(uow);

                long minDateTimestamp;
                if (numberOfDays.HasValue)
                {
                    var currentDate = DateTimeOffset.UtcNow;
                    var minDate = currentDate.AddDays(-numberOfDays.Value);
                    minDateTimestamp = minDate.ToUnixTimeMilliseconds();
                }
                else
                {
                    var configBlockResult = _blockInfoService.GetLastConfigBlock();
                    if (configBlockResult.Failed)
                        return Result.Success(Enumerable.Empty<ValidatorStatsDto>());

                    minDateTimestamp = ((DateTimeOffset)configBlockResult.Data.Timestamp).ToUnixTimeMilliseconds();
                }

                var receivedStakes = blockchainInfoRepo.GetReceivedStakes();
                var validatorRewards = blockchainInfoRepo.GetValidatorRewards(minDateTimestamp);

                var stats = new List<ValidatorStatsDto>();
                var validators = NewRepository<Validator>(uow).Get(v => !v.IsDeleted);
                var validatorAddresses = validators.Select(v => v.BlockchainAddress);
                var deposits = NewRepository<Address>(uow)
                    .GetAs(
                        a => validatorAddresses.Contains(a.BlockchainAddress),
                        a => new KeyValuePair<string, decimal>(a.BlockchainAddress, a.DepositBalance))
                    .ToDictionary(d => d.Key, d => d.Value);

                var validatorProposedBlockCount = blockchainInfoRepo.GetValidatorProposedBlockCount(minDateTimestamp);
                var validatorProposedTxCount = blockchainInfoRepo.GetValidatorProposedTxCount(minDateTimestamp);
                var validatorStakingRewards = blockchainInfoRepo.GetValidatorStakingRewards(minDateTimestamp);
                foreach (var validator in validators)
                {
                    var validatorStatsDto = new ValidatorStatsDto
                    {
                        BlockchainAddress = validator.BlockchainAddress,
                        NetworkAddress = validator.NetworkAddress,
                        IsActive = validator.IsActive,
                        SharedRewardPercent = validator.SharedRewardPercent
                    };

                    if (receivedStakes.TryGetValue(validator.BlockchainAddress, out decimal validatorStake))
                        validatorStatsDto.TotalStake = validatorStake;

                    if (validatorProposedBlockCount.TryGetValue(validator.ValidatorId, out int proposedBlockCount))
                        validatorStatsDto.BlocksProposed = proposedBlockCount;

                    if (validatorProposedTxCount.TryGetValue(validator.ValidatorId, out int proposedTxCount))
                        validatorStatsDto.TxsProposed = proposedTxCount;

                    if (validatorStakingRewards.TryGetValue(validator.ValidatorId, out decimal stakingRewards))
                        validatorStatsDto.RewardsDistributed = stakingRewards;

                    if (validatorRewards.TryGetValue(validator.BlockchainAddress, out decimal collectedRewards))
                        validatorStatsDto.RewardsCollected = collectedRewards;

                    if (deposits.TryGetValue(validator.BlockchainAddress, out decimal validatorDeposit))
                        validatorStatsDto.Deposit = validatorDeposit;

                    stats.Add(validatorStatsDto);
                }

                var orderedStats = stats
                    .OrderByDescending(s => s.TotalStake)
                    .ThenBy(s => s.BlockchainAddress)
                    .AsEnumerable();

                return Result.Success(orderedStats);
            }
        }

        public Result<AddressSummaryDto> GetTopAddresses(int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var addressRepo = NewRepository<Address>(uow);

                var addressesWithHolding =
                    addressRepo.GetCount(a => a.AvailableBalance + a.DepositBalance + a.StakedBalance > 0);

                var topAddresses = addressRepo
                    .GetLastAs(
                        a => a.AvailableBalance + a.DepositBalance + a.StakedBalance > 0,
                        a => a.AvailableBalance + a.DepositBalance + a.StakedBalance,
                        a => new AddressInfoSlimDto
                        {
                            BlockchainAddress = a.BlockchainAddress,
                            TotalBalance = a.AvailableBalance + a.DepositBalance + a.StakedBalance
                        },
                        (page - 1) * limit,
                        limit)
                   .ToList();

                return Result.Success(new AddressSummaryDto
                {
                    Addresses = topAddresses,
                    AddressCount = addressesWithHolding
                });
            }
        }

        public async Task<Result<ChxSupplyDto>> GetChxSupply()
        {
            var genesisAmount = 0M;
            foreach(var address in Config.GenesisAddresses)
            {
                var addressResult = await _blockchainClient.GetAddressInfo(address);
                if (addressResult.Failed)
                    return Result.Failure<ChxSupplyDto>(addressResult.Alerts);

                genesisAmount += addressResult.Data.Balance.Total;
            }

            return Result.Success(new ChxSupplyDto
            {
                Total = Config.GenesisChxSupply.Value,
                Circulating = Config.GenesisChxSupply.Value - genesisAmount
            });
        }

        public async Task<Result<decimal>> GetTotalChxSupply()
        {
            var chxSupplyResult = await GetChxSupply();
            return chxSupplyResult.Failed
                ? Result.Failure<decimal>(chxSupplyResult.Alerts)
                : Result.Success(chxSupplyResult.Data.Total);
        }

        public async Task<Result<decimal>> GetCirculatingChxSupply()
        {
            var chxSupplyResult = await GetChxSupply();
            return chxSupplyResult.Failed
                ? Result.Failure<decimal>(chxSupplyResult.Alerts)
                : Result.Success(chxSupplyResult.Data.Circulating);
        }
    }
}
