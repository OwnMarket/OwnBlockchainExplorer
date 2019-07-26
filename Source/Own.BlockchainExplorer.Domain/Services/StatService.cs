using System;
using System.Linq;
using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Common.Extensions;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class StatService : DataService, IStatService
    {
        public StatService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<IEnumerable<KeyValuePair<DateTime, int>>> GetTxPerDay(int numberOfDays)
        {
            using (var uow = NewUnitOfWork())
            {
                var currentDate = DateTime.UtcNow.Date;
                var minDate = currentDate.AddDays(-1 * numberOfDays);

                var result = NewRepository<Transaction>(uow)
                    .Get(t => GetDate(t.Timestamp) > minDate)
                    .GroupBy(t => GetDate(t.Timestamp))
                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                    .ToList();

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

        public Result<IEnumerable<ValidatorStatsDto>> GetValidatorStats(int numberOfDays)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var receivedStakes = eventRepo
                  .GetAs(
                      e => e.EventType == EventType.Action.ToString()
                      && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                      && e.Fee == null
                      && e.Transaction.Status == TxStatus.Success.ToString()
                      && e.Amount.HasValue
                      && e.Amount.Value > 0,
                      e => new KeyValuePair<string, decimal>(e.Address.BlockchainAddress, e.Amount.Value))
                  .GroupBy(s => s.Key)
                  .ToDictionary(g => g.Key, g => g.Sum(s => s.Value));

                var currentDate = DateTime.UtcNow.Date;
                var minDate = currentDate.AddDays(-numberOfDays);

                var blocksProposed = NewRepository<Block>(uow)
                    .GetAs(b => GetDate(b.Timestamp) > minDate, b => new { b.ValidatorId, b.BlockId })
                    .GroupBy(b => b.ValidatorId)
                    .ToDictionary(g => g.Key, g => g.Select(i => i.BlockId).Distinct().ToList());

                var validatorRewards = eventRepo
                    .GetAs(
                        e => e.EventType == EventType.ValidatorReward.ToString()
                        && GetDate(e.Block.Timestamp) > minDate
                        && e.Amount.HasValue
                        && e.Amount.Value > 0,
                        e => new KeyValuePair<string, decimal>(e.Address.BlockchainAddress, e.Amount.Value))
                    .GroupBy(s => s.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.Value));

                var blockStakingRewards = eventRepo
                    .GetAs(
                        e => e.EventType == EventType.StakingReward.ToString()
                        && GetDate(e.Block.Timestamp) > minDate
                        && e.Amount.HasValue
                        && e.Amount.Value > 0,
                       e => new KeyValuePair<long, decimal>(e.BlockId, e.Amount.Value))
                    .GroupBy(s => s.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(s => s.Value));

                var stats = new List<ValidatorStatsDto>();
                var validators = NewRepository<Validator>(uow).Get(v => !v.IsDeleted);
                var validatorAddresses = validators.Select(v => v.BlockchainAddress);
                var deposits = NewRepository<Address>(uow)
                    .GetAs(
                        a => a.BlockchainAddress.ContainedIn(validatorAddresses),
                        a => new KeyValuePair<string, decimal>(a.BlockchainAddress, a.DepositBalance))
                    .ToDictionary(d => d.Key, d => d.Value);

                foreach (var validator in validators)
                {
                    var validatorStatsDto = new ValidatorStatsDto
                    {
                        BlockchainAddress = validator.BlockchainAddress,
                        NetworkAddress = validator.NetworkAddress,
                        SharedRewardPercent = validator.SharedRewardPercent
                    };

                    if (receivedStakes.TryGetValue(validator.BlockchainAddress, out decimal validatorStake))
                        validatorStatsDto.TotalStake = validatorStake;

                    if (blocksProposed.TryGetValue(validator.ValidatorId, out List<long> blockIdsProposed))
                    {
                        validatorStatsDto.BlocksProposed = blockIdsProposed.Count();

                        var txsProposedCount = eventRepo
                            .GetCount(
                                e => e.TransactionId.HasValue
                                && e.BlockId.ContainedIn(blockIdsProposed));
                        validatorStatsDto.TxsProposed = txsProposedCount;

                        var stakingRewards = blockStakingRewards
                            .Where(b => b.Key.ContainedIn(blockIdsProposed))
                            .Sum(b => b.Value);
                        validatorStatsDto.RewardsDistributed = stakingRewards;
                    }

                    if (validatorRewards.TryGetValue(validator.BlockchainAddress, out decimal collectedRewards))
                        validatorStatsDto.RewardsCollected = collectedRewards;

                    if (deposits.TryGetValue(validator.BlockchainAddress, out decimal validatorDeposit))
                        validatorStatsDto.Deposit = validatorDeposit;

                    stats.Add(validatorStatsDto);
                }
                return Result.Success(stats.AsEnumerable());
            }
        }

        public Result<IEnumerable<KeyValuePair<string, decimal>>> GetTopAddresses(int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var topAddresses = NewRepository<Address>(uow)
                    .GetLastAs(
                        a => GetTotalBalance(a) > 0,
                        a => GetTotalBalance(a),
                        a => new KeyValuePair<string, decimal>(a.BlockchainAddress, GetTotalBalance(a)),
                        (page - 1) * limit,
                        limit);

                return Result.Success(topAddresses);
            }
        }

        private decimal GetTotalBalance(Address a)
        {
            return a.AvailableBalance + a.DepositBalance + a.StakedBalance;
        }

        private DateTime GetDate(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(timestamp).Date;
        }
    }
}
