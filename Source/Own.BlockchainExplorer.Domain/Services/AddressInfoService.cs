using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class AddressInfoService : DataService, IAddressInfoService
    {
        public AddressInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<IEnumerable<ControlledAccountDto>> GetAccountsInfo(string blockchainAddress, 
            int page, 
            int limit, 
            bool? isActive)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var events = eventRepo
                    .Get(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && (e.TxAction.ActionType == ActionType.CreateAccount.ToString()
                        || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                        && e.Transaction.Status == TxStatus.Success.ToString(),
                        e => e.Account,
                        e => e.Address,
                        e => e.TxAction,
                        e => e.Transaction);

                if (!events.Any())
                    return Result.Success(new List<ControlledAccountDto>().AsEnumerable());

                return Result.Success(
                    events.Select(e => new ControlledAccountDto
                    {
                        Hash = e.Account.Hash,
                        IsActive = e.Account.ControllerAddress == events.First().Address.BlockchainAddress
                    })
                    .Distinct(new ControlledAccountDtoEqualityComparer())
                    .Where(e => isActive.HasValue ? e.IsActive == isActive : true)
                    .Skip((page - 1) * limit).Take(limit)
                );
            }
        }

        public Result<IEnumerable<ControlledAssetDto>> GetAssetsInfo(string blockchainAddress, 
            int page, 
            int limit,
            bool? isActive)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var events = eventRepo
                    .Get(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && (e.TxAction.ActionType == ActionType.CreateAsset.ToString()
                        || e.TxAction.ActionType == ActionType.SetAssetController.ToString())
                        && e.Transaction.Status == TxStatus.Success.ToString(),
                        e => e.Asset,
                        e => e.Address,
                        e => e.TxAction,
                        e => e.Transaction);

                if (!events.Any())
                    return Result.Success(new List<ControlledAssetDto>().AsEnumerable());

                return Result.Success(
                    events.Select(e => new ControlledAssetDto
                    {
                        Hash = e.Asset.Hash,
                        AssetCode = e.Asset.AssetCode,
                        IsActive = e.Asset.ControllerAddress == events.First().Address.BlockchainAddress
                    })
                    .Distinct(new ControlledAssetDtoEqualityComparer())
                    .Where(e => isActive.HasValue ? e.IsActive == isActive : true)
                    .Skip((page - 1) * limit).Take(limit)
                );
            }
        }

        public Result<IEnumerable<StakeDto>> GetDelegatedStakesInfo(string blockchainAddress, 
            int page, 
            int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var delegateStakeIds = eventRepo
                    .GetAs(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Fee != null
                        && e.Transaction.Status == TxStatus.Success.ToString(),
                        e => e.TxActionId);

                if (!delegateStakeIds.Any())
                    return Result.Success(new List<StakeDto>().AsEnumerable());

                return Result.Success(eventRepo
                    .Get(e => delegateStakeIds.Contains(e.TxActionId) && e.Fee == null, e => e.Address)
                    .OrderByDescending(e => e.BlockchainEventId)
                    .GroupBy(e => e.Address)
                    .Where(g => g.Sum(e => e.Amount.Value) != 0)
                    .Skip((page - 1) * limit).Take(limit)
                    .Select(g => new StakeDto
                    {
                        ValidatorAddress = g.Key.BlockchainAddress,
                        Amount = g.Sum(e => e.Amount.Value),
                        StakerAddress = blockchainAddress
                    }));
            }
        }

        public Result<IEnumerable<StakeDto>> GetReceivedStakesInfo(string blockchainAddress, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var receivedStakeIds = eventRepo
                    .Get(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Fee == null
                        && e.Transaction.Status == TxStatus.Success.ToString(),
                        e => e.Account,
                        e => e.Address,
                        e => e.TxAction,
                        e => e.Transaction)
                    .Select(e => e.TxActionId);

                if (!receivedStakeIds.Any())
                    return Result.Success(new List<StakeDto>().AsEnumerable());

                return Result.Success(eventRepo
                    .Get(e => receivedStakeIds.Contains(e.TxActionId) && e.Fee != null, e => e.Address)
                    .OrderByDescending(e => e.BlockchainEventId)
                    .GroupBy(e => e.Address)
                    .Where (g => g.Sum(e => e.Amount.Value) != 0)
                    .Skip((page - 1) * limit).Take(limit)
                    .Select(g => new StakeDto
                    {
                        StakerAddress = g.Key.BlockchainAddress,
                        Amount = g.Sum(e => e.Amount.Value) * -1,
                        ValidatorAddress = blockchainAddress
                    }));
            }
        }

        public Result<IEnumerable<EventDto>> GetEventsInfo(string blockchainAddress, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    NewRepository<BlockchainEvent>(uow)
                    .Get(
                        e => 
                            e.Address.BlockchainAddress == blockchainAddress
                            && !((e.EventType == EventType.ValidatorReward.ToString()
                            || e.EventType == EventType.StakingReward.ToString())
                            && e.Amount == 0),
                        e => e.TxAction,
                        e => e.Equivocation,
                        e => e.Transaction,
                        e => e.Block)
                    .OrderByDescending(e => e.BlockchainEventId)
                    .Skip((page - 1) * limit).Take(limit)
                    .Select(e => EventDto.FromDomainModel(e))
                );
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

                var addressDto = AddressInfoDto.FromDomainModel(address);

                return Result.Success(addressDto);
            }
        }
    }
}
