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

        public Result<IEnumerable<ControlledAccountDto>> GetAccountsInfo(string blockchainAddress)
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
                );
            }
        }

        public Result<IEnumerable<ControlledAssetDto>> GetAssetsInfo(string blockchainAddress)
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
                );
            }
        }

        public Result<IEnumerable<StakeDto>> GetDelegatedStakesInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var delegateStakeIds = eventRepo
                    .Get(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Amount < 0
                        && e.Transaction.Status == TxStatus.Success.ToString(),
                        e => e.Account,
                        e => e.Address,
                        e => e.TxAction,
                        e => e.Transaction)
                    .Select(e => e.TxActionId);

                if (!delegateStakeIds.Any())
                    return Result.Success(new List<StakeDto>().AsEnumerable());

                return Result.Success(eventRepo
                    .Get(e => delegateStakeIds.Contains(e.TxActionId) && e.Amount > 0, e => e.Address)
                    .Select(e => new StakeDto
                    {
                        ValidatorAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value,
                        StakerAddress = blockchainAddress
                    }));
            }
        }

        public Result<IEnumerable<StakeDto>> GetReceivedStakesInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var receivedStakeIds = eventRepo
                    .Get(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Amount > 0
                        && e.Transaction.Status == TxStatus.Success.ToString(),
                        e => e.Account,
                        e => e.Address,
                        e => e.TxAction,
                        e => e.Transaction)
                    .Select(e => e.TxActionId);

                if (!receivedStakeIds.Any())
                    return Result.Success(new List<StakeDto>().AsEnumerable());

                return Result.Success(eventRepo
                    .Get(e => receivedStakeIds.Contains(e.TxActionId) && e.Amount < 0, e => e.Address)
                    .Select(e => new StakeDto
                    {
                        StakerAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value * -1,
                        ValidatorAddress = blockchainAddress
                    }));
            }
        }

        public Result<EventsDto> GetEventsInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var events = eventRepo.Get(
                    e => e.Address.BlockchainAddress == blockchainAddress,
                    e => e.Address,
                    e => e.TxAction,
                    e => e.Equivocation,
                    e => e.Transaction);

                var eventsDto = new EventsDto
                {
                    StakingRewards = events
                    .Where(e => e.EventType == EventType.StakingReward.ToString())
                    .Select(e => new StakingRewardDto
                    {
                        StakerAddress = blockchainAddress,
                        Amount = e.Amount.Value
                    })
                    .ToList(),

                    ValidatorRewards = events
                    .Where(e => e.EventType == EventType.ValidatorReward.ToString())
                    .Select(e => new ValidatorRewardDto
                    {
                        Amount = e.Amount.Value
                    })
                    .ToList(),

                    TakenDeposits = events
                    .Where(e => e.EventType == EventType.DepositTaken.ToString())
                    .Select(e => new DepositDto
                    {
                        BlockchainAddress = blockchainAddress,
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                        Amount = e.Amount.Value * -1
                    })
                    .ToList(),

                    GivenDeposits = events
                    .Where(e => e.EventType == EventType.DepositGiven.ToString())
                    .Select(e => new DepositDto
                    {
                        BlockchainAddress = blockchainAddress,
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                        Amount = e.Amount.Value
                    })
                    .ToList(),

                    Actions = events
                    .Where(e => e.EventType == EventType.Action.ToString())
                    .Select(e => new ActionDto
                    {
                        ActionNumber = e.TxAction.ActionNumber,
                        ActionType = e.TxAction.ActionType,
                        ActionData = e.TxAction.ActionData,
                        TxHash = e.Transaction.Hash
                    })
                    .Distinct(new ActionDtoEqualityComparer())
                    .ToList()
                };

                return Result.Success(eventsDto);
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
