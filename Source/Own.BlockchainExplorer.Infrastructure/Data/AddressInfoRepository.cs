using System;
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
                    && e.Transaction.Status == TxStatus.Success.ToString())
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
                    && e.Transaction.Status == TxStatus.Success.ToString())
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
                    (e.EventType == EventType.Action.ToString()
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Fee != null
                    || e.EventType == EventType.StakeReturned.ToString())
                    && e.Address.BlockchainAddress == blockchainAddress
                    && e.Transaction.Status == TxStatus.Success.ToString())
                .Select(e => e.TxActionId)
                .ToList();

            if (!delegateStakeIds.Any())
                return new StakeSummaryDto
                {
                    Stakes = new List<StakeDto>(),
                    TotalAmount = 0
                };

            var stakesQuery = _db.BlockchainEvents
                .Where(e =>
                    delegateStakeIds.Contains(e.TxActionId)
                    && e.Fee == null
                    && e.Address.BlockchainAddress != blockchainAddress)
                .Include(e => e.Address)
                .OrderByDescending(e => e.BlockchainEventId)
                .GroupBy(e => e.Address)
                .Where(g => g.Sum(e => e.Amount.Value) != 0);

            var stakes = stakesQuery
                .Select(g => new StakeDto
                {
                    ValidatorAddress = g.Key.BlockchainAddress,
                    Amount = g.Sum(e => e.Amount.Value),
                    StakerAddress = blockchainAddress
                })
                .OrderByDescending(s => s.Amount)
                .ThenBy(s => s.StakerAddress)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var totalStakeAmount = stakesQuery
                .Select(g => g.Sum(e => e.Amount.Value))
                .Sum();

            return new StakeSummaryDto
            {
                Stakes = stakes,
                TotalAmount = totalStakeAmount
            };
        }

        public StakeSummaryDto GetReceivedStakesInfo(string blockchainAddress, int page, int limit)
        {
            var receivedStakeIds = _db.BlockchainEvents
                .Where(e =>
                    (e.EventType == EventType.Action.ToString()
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                    || e.EventType == EventType.StakeReturned.ToString())
                    && e.Address.BlockchainAddress == blockchainAddress
                    && e.Fee == null
                    && e.Transaction.Status == TxStatus.Success.ToString())
                .Select(e => e.TxActionId)
                .ToList();

            if (!receivedStakeIds.Any())
                return new StakeSummaryDto
                {
                    Stakes = new List<StakeDto>(),
                    TotalAmount = 0
                };

            var stakesQuery = _db.BlockchainEvents
                .Where(e =>
                    receivedStakeIds.Contains(e.TxActionId)
                    && (e.Fee != null
                        || e.EventType == EventType.StakeReturned.ToString() && e.Fee == null)
                    && e.Address.BlockchainAddress != blockchainAddress)
                .Include(e => e.Address)
                .OrderByDescending(e => e.BlockchainEventId)
                .GroupBy(e => e.Address)
                .Where(g => g.Sum(e => e.Amount.Value) != 0);

            var stakes = stakesQuery
                .Select(g => new StakeDto
                {
                    StakerAddress = g.Key.BlockchainAddress,
                    Amount = -g.Sum(e => e.Amount.Value),
                    ValidatorAddress = blockchainAddress
                })
                .OrderByDescending(s => s.Amount)
                .ThenBy(s => s.StakerAddress)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var totalStakeAmount = stakesQuery
                .Select(g => -g.Sum(e => e.Amount.Value))
                .Sum();

            return new StakeSummaryDto
            {
                Stakes = stakes,
                TotalAmount = totalStakeAmount
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
                .Include(e => e.Transaction)
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
