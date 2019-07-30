using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
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
            var events =
                _db.BlockchainEvents.AsQueryable()
                .Where(
                    e => e.EventType == EventType.Action.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && (e.TxAction.ActionType == ActionType.CreateAccount.ToString()
                    || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                    && e.Transaction.Status == TxStatus.Success.ToString())
                .Include(e => e.Account)
                .Include(e => e.Address)
                .ToList();

            if (!events.Any())
                return Enumerable.Empty<ControlledAccountDto>();

            var controllerAddress = events.First().Address.BlockchainAddress;
            return
                events.Select(e => new ControlledAccountDto
                {
                    Hash = e.Account.Hash,
                    IsActive = e.Account.ControllerAddress == controllerAddress
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
            var events =
                _db.BlockchainEvents.AsQueryable()
                .Where(
                    e => e.EventType == EventType.Action.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && (e.TxAction.ActionType == ActionType.CreateAsset.ToString()
                    || e.TxAction.ActionType == ActionType.SetAssetController.ToString())
                    && e.Transaction.Status == TxStatus.Success.ToString())
                .Include(e => e.Asset)
                .Include(e => e.Address)
                .ToList();

            if (!events.Any())
                return Enumerable.Empty<ControlledAssetDto>();

            var controllerAddress = events.First().Address.BlockchainAddress;
            return
                events.Select(e => new ControlledAssetDto
                {
                    Hash = e.Asset.Hash,
                    AssetCode = e.Asset.AssetCode,
                    IsActive = e.Asset.ControllerAddress == controllerAddress
                })
                .GroupBy(c => c.Hash)
                .Select(g => g.FirstOrDefault())
                .Where(e => isActive.HasValue ? e.IsActive == isActive : true)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        }

        public IEnumerable<StakeDto> GetDelegatedStakesInfo(string blockchainAddress, int page, int limit)
        {
            var delegateStakeIds =
                _db.BlockchainEvents.AsQueryable()
                .Where(
                    e => e.EventType == EventType.Action.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                    && e.Fee != null
                    && e.Transaction.Status == TxStatus.Success.ToString())
                .Select(e => e.TxActionId)
                .ToList();

            if (!delegateStakeIds.Any())
                return Enumerable.Empty<StakeDto>();

            return
                _db.BlockchainEvents.AsQueryable()
                .Where(e => delegateStakeIds.Contains(e.TxActionId) && e.Fee == null)
                .Include(e => e.Address)
                .OrderByDescending(e => e.BlockchainEventId)
                .GroupBy(e => e.Address)
                .Where(g => g.Sum(e => e.Amount.Value) != 0)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(g => new StakeDto
                {
                    ValidatorAddress = g.Key.BlockchainAddress,
                    Amount = g.Sum(e => e.Amount.Value),
                    StakerAddress = blockchainAddress
                })
                .ToList();
        }

        public IEnumerable<StakeDto> GetReceivedStakesInfo(string blockchainAddress, int page, int limit)
        {
            var receivedStakeIds =
                _db.BlockchainEvents.AsQueryable()
                .Where(
                    e => e.EventType == EventType.Action.ToString()
                    && e.Address.BlockchainAddress == blockchainAddress
                    && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                    && e.Fee == null
                    && e.Transaction.Status == TxStatus.Success.ToString())
                .Select(e => e.TxActionId)
                .ToList();

            if (!receivedStakeIds.Any())
                return Enumerable.Empty<StakeDto>();

            return
                _db.BlockchainEvents.AsQueryable()
                .Where(e => receivedStakeIds.Contains(e.TxActionId) && e.Fee != null)
                .Include(e => e.Address)
                .OrderByDescending(e => e.BlockchainEventId)
                .GroupBy(e => e.Address)
                .Where(g => g.Sum(e => e.Amount.Value) != 0)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(g => new StakeDto
                {
                    StakerAddress = g.Key.BlockchainAddress,
                    Amount = -g.Sum(e => e.Amount.Value),
                    ValidatorAddress = blockchainAddress
                })
                .ToList();
        }

        private bool IsEmptyReward(BlockchainEvent e)
        {
            return (e.EventType == EventType.ValidatorReward.ToString()
                || e.EventType == EventType.StakingReward.ToString())
                && e.Amount == 0;
        }

        public EventsSummaryDto GetEventsInfo(string blockchainAddress, int page, int limit)
        {
            var query =
                 _db.BlockchainEvents.AsQueryable()
                 .Where(e => e.Address.BlockchainAddress == blockchainAddress
                     && !IsEmptyReward(e));

            var eventsCount =
                query.Select(e => e.BlockchainEventId).Count();

            var events =
                query
                .Include(e => e.TxAction)
                .Include(e => e.Equivocation)
                .Include(e => e.Transaction)
                .Include(e => e.Block)
                .OrderByDescending(e => e.BlockchainEventId)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList()
                .Select(e => EventDto.FromDomainModel(e))
                .ToList();

            return new EventsSummaryDto
            {
                Events = events,
                EventsCount = eventsCount
            };
        }
    }
}
