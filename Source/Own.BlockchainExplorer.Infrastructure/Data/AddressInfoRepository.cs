﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Infrastructure.Data.EF;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class AddressInfoRepository : IAddressInfoRepository
    {
        private readonly OwnDb _db;
        public AddressInfoRepository(OwnDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IEnumerable<ControlledAccountDto> GetAccountsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive)
        {
            return _db.BlockchainEvents
                .Where(e =>
                    e.EventType == EventType.Action.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && (e.TxAction.ActionType == ActionType.CreateAccount.ToString()
                        || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                    && e.Tx.Status == TxStatus.Success.ToString())
                .Include(e => e.Account)
                .Include(e => e.Address)
                .Select(e => new ControlledAccountDto
                {
                    Hash = e.Account.Hash,
                    IsActive = e.Account.ControllerAddress == blockchainAddress
                })
                .GroupBy(c => c.Hash)
                .Select(g => g.FirstOrDefault())
                .Where(e => isActive.HasValue ? e.IsActive == isActive : true)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        }

        public IEnumerable<ControlledAssetDto> GetAssetsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive)
        {
            return _db.BlockchainEvents
                .Where(e =>
                    e.EventType == EventType.Action.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && (e.TxAction.ActionType == ActionType.CreateAsset.ToString()
                        || e.TxAction.ActionType == ActionType.SetAssetController.ToString())
                    && e.Tx.Status == TxStatus.Success.ToString())
                .Include(e => e.Asset)
                .Include(e => e.Address)
                .Select(e => new ControlledAssetDto
                {
                    Hash = e.Asset.Hash,
                    AssetCode = e.Asset.AssetCode,
                    IsActive = e.Asset.ControllerAddress == blockchainAddress
                })
                .GroupBy(c => c.Hash)
                .Select(g => g.FirstOrDefault())
                .Where(e => isActive.HasValue ? e.IsActive == isActive : true)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        }

        public StakeSummaryDto GetDelegatedStakesInfo(string blockchainAddress, int page, int limit)
        {
            var delegateStakeIds = _db.BlockchainEvents
                .Where(e =>
                    e.EventType == EventType.Action.ToString()
                    && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                    && e.Fee != null
                    && e.Address.BlockchainAddress == blockchainAddress
                    && e.Tx.Status == TxStatus.Success.ToString())
                .Select(e => e.TxActionId)
                .ToList();

            if (!delegateStakeIds.Any())
                return new StakeSummaryDto
                {
                    Stakes = new List<StakeDto>(),
                    TotalAmount = 0
                };

            var stakes = _db.BlockchainEvents
                .Where(e => delegateStakeIds.Contains(e.TxActionId) && e.Fee == null)
                .Include(e => e.Address)
                .GroupBy(e => e.Address)
                .Select(g => new StakeDto
                {
                    ValidatorAddress = g.Key.BlockchainAddress,
                    Amount = g.Sum(e => e.Amount.Value),
                    StakerAddress = blockchainAddress
                })
                .ToList();

            var stakeReturnedEvents = _db.BlockchainEvents
            .Where(e =>
                e.EventType == EventType.StakeReturned.ToString()
                && e.Address.BlockchainAddress == blockchainAddress
                && e.Amount > 0
                && e.GroupingId != null)
            .ToList();

            var stakeReturnedGroupingIds = stakeReturnedEvents.Select(e => e.GroupingId);

            var stakeReturnedValidatorEvents = _db.BlockchainEvents
                .Where(e => stakeReturnedGroupingIds.Contains(e.GroupingId) && e.Amount < 0)
                .Include(e => e.Address)
                .ToList();

            foreach(var stake in stakes)
            {
                var groupingIds = stakeReturnedValidatorEvents
                    .Where(s => s.Address.BlockchainAddress == stake.ValidatorAddress)
                    .Select(e => e.GroupingId);

                var stakesReturnedAmount = stakeReturnedEvents
                    .Where(e => groupingIds.Contains(e.GroupingId))
                    .Sum(e => e.Amount) ?? 0;

                stake.Amount -= stakesReturnedAmount;
            }

            stakes = stakes
                .Where(s => s.Amount != 0)
                .OrderByDescending(s => s.Amount)
                .ThenBy(s => s.ValidatorAddress)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return new StakeSummaryDto
            {
                Stakes = stakes,
                TotalAmount = stakes.Sum(s => s.Amount)
            };
        }

        public StakeSummaryDto GetReceivedStakesInfo(string blockchainAddress, int page, int limit)
        {
            var receivedStakeIds = _db.BlockchainEvents
                .Where(e =>
                    e.EventType == EventType.Action.ToString()
                    && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && e.Fee == null
                    && e.Tx.Status == TxStatus.Success.ToString())
                .Select(e => e.TxActionId)
                .ToList();

            if (!receivedStakeIds.Any())
                return new StakeSummaryDto
                {
                    Stakes = new List<StakeDto>(),
                    TotalAmount = 0
                };

            var stakes = _db.BlockchainEvents
                .Where(e => receivedStakeIds.Contains(e.TxActionId) && e.Fee != null)
                .Include(e => e.Address)
                .GroupBy(e => e.Address)
                .Select(g => new StakeDto
                {
                    StakerAddress = g.Key.BlockchainAddress,
                    Amount = -g.Sum(e => e.Amount.Value),
                    ValidatorAddress = blockchainAddress
                })
                .ToList();

            var stakeReturnedGroupingIds = _db.BlockchainEvents
            .Where(e =>
               e.EventType == EventType.StakeReturned.ToString()
               && e.Address.BlockchainAddress == blockchainAddress
               && e.Amount < 0
               && e.GroupingId != null)
            .Select(e => e.GroupingId)
            .ToList();

            var stakeReturnedStakerEvents = _db.BlockchainEvents
                .Where(e => stakeReturnedGroupingIds.Contains(e.GroupingId) && e.Amount > 0)
                .Include(e => e.Address)
                .ToList();

            foreach (var stake in stakes)
            {
                var stakesReturnedAmount = stakeReturnedStakerEvents
                    .Where(s => s.Address.BlockchainAddress == stake.StakerAddress)
                    .Sum(s => s.Amount) ?? 0;

                stake.Amount -= stakesReturnedAmount;
            }

            stakes = stakes
                .Where(s => s.Amount != 0)
                .OrderByDescending(s => s.Amount)
                .ThenBy(s => s.StakerAddress)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return new StakeSummaryDto
            {
                Stakes = stakes,
                TotalAmount = stakes.Sum(s => s.Amount)
            };
        }

        public EventsSummaryDto GetEventsInfo(string blockchainAddress, string filter, int page, int limit)
        {
            var eventTypes = filter.Split(',')
                .Where(f => Enum.TryParse(f, out EventType result))
                .ToList();

            var validatorReward = EventType.ValidatorReward.ToString();
            var stakingReward = EventType.StakeReturned.ToString();

            var query = _db.BlockchainEvents.AsQueryable()
                .Where(e =>
                    e.Address.BlockchainAddress == blockchainAddress
                    && !(
                        (e.EventType == validatorReward || e.EventType == stakingReward)
                        && e.Amount == 0
                    )
                );

            if (eventTypes.Any())
                query = query.Where(e => eventTypes.Contains(e.EventType));

            var eventsCount =
                query.Select(e => e.BlockchainEventId).Count();

            var events = query
                .Include(e => e.TxAction)
                .Include(e => e.Equivocation)
                .Include(e => e.Tx)
                .Include(e => e.Block)
                .OrderByDescending(e => e.BlockchainEventId)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList()
                .Select(EventDto.FromDomainModel)
                .ToList();

            return new EventsSummaryDto
            {
                Events = events,
                EventsCount = eventsCount
            };
        }
    }
}
