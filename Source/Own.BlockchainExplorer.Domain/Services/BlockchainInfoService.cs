using System.Linq;
using System;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class BlockchainInfoService : DataService, IBlockchainInfoService
    {
        private readonly IBlockchainInfoRepositoryFactory _blockchainInfoRepositoryFactory;

        public BlockchainInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IBlockchainInfoRepositoryFactory blockchainInfoRepositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainInfoRepositoryFactory = blockchainInfoRepositoryFactory;
        }

        public Result<AddressInfoDto> GetAddressInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var events = eventRepo.Get(
                    e => e.Address.BlockchainAddress == blockchainAddress,
                    e => e.Account,
                    e => e.Asset,
                    e => e.Address,
                    e => e.TxAction,
                    e => e.Equivocation,
                    e => e.Transaction);

                if (!events.Any())
                    return Result.Failure<AddressInfoDto>("Address {0} does not exist.".F(blockchainAddress));

                var address = events.First().Address;

                var addressDto = AddressInfoDto.FromDomainModel(address);

                addressDto.Accounts = events
                    .Where(e => (e.TxAction.ActionType == ActionType.CreateAccount.ToString()
                        || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                        && e.Transaction.Status == TxStatus.Success.ToString())
                    .Select(e => new ControlledAccountDto {
                        Hash = e.Account.Hash,
                        IsActive = e.Account.ControllerAddress == address.BlockchainAddress})
                    .Distinct(new ControlledAccountDtoEqualityComparer())
                    .ToList();

                addressDto.Assets = events
                    .Where(e => (e.TxAction.ActionType == ActionType.CreateAsset.ToString()
                        || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                        && e.Transaction.Status == TxStatus.Success.ToString())
                    .Select(e => new ControlledAssetDto {
                        Hash = e.Asset.Hash,
                        AssetCode = e.Asset.AssetCode,
                        IsActive = e.Asset.ControllerAddress == address.BlockchainAddress })
                    .Distinct(new ControlledAssetDtoEqualityComparer())
                    .ToList();

                var delegateStakeIds = events
                    .Where(e => e.TxAction.ActionType == ActionType.DelegateStake.ToString() && e.Amount < 0)
                    .Select(e => e.TxActionId);

                addressDto.DelegatedStakes = eventRepo
                    .Get(e => delegateStakeIds.Contains(e.TxActionId) && e.Amount > 0, e => e.Address)
                    .Select(e => new StakeDto {
                        ValidatorAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value,
                        StakerAddress = address.BlockchainAddress })
                    .ToList();

                var receivedStakeIds = events
                    .Where(e => e.TxAction.ActionType == ActionType.DelegateStake.ToString() && e.Amount > 0)
                    .Select(e => e.TxActionId);

                addressDto.ReceivedStakes = eventRepo
                    .Get(e => receivedStakeIds.Contains(e.TxActionId) && e.Amount < 0, e => e.Address)
                    .Select(e => new StakeDto {
                        StakerAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value * -1,
                        ValidatorAddress = address.BlockchainAddress })
                    .ToList();

                addressDto.StakingRewards = events
                    .Where(e => e.EventType == EventType.StakingReward.ToString())
                    .Select(e => new StakingRewardDto
                    {
                        StakerAddress = address.BlockchainAddress,
                        Amount = e.Amount.Value
                    })
                    .ToList();

                addressDto.ValidatorRewards = events
                    .Where(e => e.EventType == EventType.ValidatorReward.ToString())
                    .Select(e => new ValidatorRewardDto
                    {
                        Amount = e.Amount.Value
                    })
                    .ToList();

                addressDto.TakenDeposits = events
                    .Where(e => e.EventType == EventType.DepositTaken.ToString())
                    .Select(e => new DepositDto
                    {
                        BlockchainAddress = address.BlockchainAddress,
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                        Amount = e.Amount.Value * -1
                    })
                    .ToList();

                addressDto.GivenDeposits = events
                    .Where(e => e.EventType == EventType.DepositGiven.ToString())
                    .Select(e => new DepositDto
                    {
                        BlockchainAddress = address.BlockchainAddress,
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                        Amount = e.Amount.Value
                    })
                    .ToList();

                addressDto.Actions = events
                    .Where(e => e.EventType == EventType.Action.ToString())
                    .Select(e => new ActionDto
                    {
                        ActionNumber = e.TxAction.ActionNumber,
                        ActionType = e.TxAction.ActionType,
                        ActionData = e.TxAction.ActionData,
                        TxHash = e.Transaction.Hash
                    })
                    .Distinct(new ActionDtoEqualityComparer())
                    .ToList();

                return Result.Success(addressDto);
            }
        }

        public Result<BlockInfoDto> GetBlockInfo(long blockNumber)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Block.BlockNumber == blockNumber, 
                    e => e.Equivocation, 
                    e => e.Address, 
                    e => e.Block.Validator, 
                    e => e.Transaction);
                if (!events.Any())
                    return Result.Failure<BlockInfoDto>("Block {0} does not exist.".F(blockNumber));

                var blockDto = BlockInfoDto.FromDomainModel(events.First().Block);
                blockDto.Equivocations = events
                    .Where(e => e.EventType == EventType.DepositTaken.ToString())
                    .Select(e => new EquivocationInfoShortDto
                    {
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                        TakenDeposit = new DepositDto {
                            BlockchainAddress = e.Address.BlockchainAddress,
                            Amount = e.Amount.Value * -1,
                            EquivocationProofHash = e.Equivocation.EquivocationProofHash}
                    }).ToList();

                blockDto.StakingRewards = events
                    .Where(e => e.EventType == EventType.StakingReward.ToString())
                    .Select(e => new StakingRewardDto {
                        StakerAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value })
                    .ToList();

                blockDto.Transactions = events
                    .Where(e => e.EventType == EventType.Action.ToString())
                    .GroupBy(e => e.Transaction)
                    .Select(g => new TxInfoShortDto {
                        Hash = g.Key.Hash,
                        NumberOfActions = g.Select(e => e.TxActionId).Distinct().Count(),
                        SenderAddress = g.First().Address.BlockchainAddress,
                        Timestamp = blockDto.Timestamp,
                        BlockNumber = blockDto.BlockNumber
                    })   
                    .ToList();

                return Result.Success(blockDto);
            }
        }

        public Result<TxInfoDto> GetTxInfo(string txHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Transaction.Hash == txHash, 
                    e => e.Transaction, 
                    e => e.TxAction, 
                    e => e.Block, 
                    e => e.Address);
                if (!events.Any())
                    return Result.Failure<TxInfoDto>("Transaction {0} does not exist.".F(txHash));

                var txDto = TxInfoDto.FromDomainModel(events.FirstOrDefault().Transaction);
                txDto.Actions = events
                    .GroupBy(e => e.TxActionId)
                    .Select(g => ActionDto.FromDomainModel(g.First().TxAction)).ToList();
                txDto.BlockNumber = events.FirstOrDefault().Block.BlockNumber;
                txDto.SenderAddress = events.FirstOrDefault().Address.BlockchainAddress;

                return Result.Success(txDto);
            }
        }

        public Result<EquivocationInfoDto> GetEquivocationInfo(string equivocationProofHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Equivocation.EquivocationProofHash == equivocationProofHash,
                    e => e.Equivocation,
                    e => e.Address);
                if (!events.Any()) return Result.Failure<EquivocationInfoDto>(
                    "Equivocation {0} does not exist.".F(equivocationProofHash));

                var eqDto = EquivocationInfoDto.FromDomainModel(events.First().Equivocation);

                var depositTakenEvent = events.Single(e => e.EventType == EventType.DepositTaken.ToString());
                eqDto.TakenDeposit = new DepositDto
                {
                    BlockchainAddress = depositTakenEvent.Address.BlockchainAddress,
                    Amount = depositTakenEvent.Amount.Value * -1
                };

                eqDto.GivenDeposits = events
                    .Where(e => e.EventType == EventType.DepositGiven.ToString())
                    .Select(e => new DepositDto
                    {
                        BlockchainAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value
                    }).ToList();

                return Result.Success(eqDto);
            }
        }

        public Result<AccountInfoDto> GetAccountInfo(string accountHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Account.Hash == accountHash,
                    e => e.TxAction,
                    e => e.Address,
                    e => e.Account.HoldingEligibilitiesByAccountId);

                if (!events.Any())
                    return Result.Failure<AccountInfoDto>("Account {0} does not exist.".F(accountHash));

                var account = events.First().Account;

                var accountDto = AccountInfoDto.FromDomainModel(account);

                accountDto.Holdings = account.HoldingEligibilitiesByAccountId
                    .Where(h => h.Balance.HasValue)
                    .Select(h => new HoldingDto { AssetHash = h.AssetHash, Balance = h.Balance.Value })
                    .ToList();

                accountDto.Eligibilities = account.HoldingEligibilitiesByAccountId
                    .Where(h => h.IsPrimaryEligible.HasValue || h.KycControllerAddress != null)
                    .Select(h => new EligibilityDto {
                        AssetHash = h.AssetHash,
                        IsPrimaryEligible = h.IsPrimaryEligible,
                        IsSecondaryEligible = h.IsSecondaryEligible,
                        KycControllerAddress = h.KycControllerAddress })
                    .ToList();

                accountDto.ControllerAddresses = events
                    .Where(e => e.TxAction.ActionType == ActionType.CreateAccount.ToString()
                        || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                    .Select(e => e.Address.BlockchainAddress).Distinct()
                    .Select(s => new ControllerAddressDto { BlockchainAddress = s }).ToList();

                return Result.Success(accountDto);
            }
        }

        public Result<AssetInfoDto> GetAssetInfo(string assetHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Asset.Hash == assetHash, 
                    e => e.TxAction, 
                    e => e.Address, 
                    e => e.Asset.HoldingEligibilitiesByAssetId);

                if (!events.Any())
                    return Result.Failure<AssetInfoDto>("Asset {0} does not exist.".F(assetHash));

                var asset = events.First().Asset;

                var assetDto = AssetInfoDto.FromDomainModel(asset);

                assetDto.Holdings = asset.HoldingEligibilitiesByAssetId
                    .Where(h => h.Balance.HasValue)
                    .Select(h => new HoldingDto { AccountHash = h.AccountHash, Balance = h.Balance.Value })
                    .ToList();

                assetDto.Eligibilities = asset.HoldingEligibilitiesByAssetId
                    .Where(h => h.IsPrimaryEligible.HasValue || h.KycControllerAddress != null)
                    .Select(h => new EligibilityDto
                    {
                        AccountHash = h.AccountHash,
                        IsPrimaryEligible = h.IsPrimaryEligible,
                        IsSecondaryEligible = h.IsSecondaryEligible,
                        KycControllerAddress = h.KycControllerAddress
                    })
                    .ToList();

                assetDto.ControllerAddresses = events
                    .Where(e => e.TxAction.ActionType == ActionType.CreateAsset.ToString()
                        || e.TxAction.ActionType == ActionType.SetAssetController.ToString())
                    .Select(e => e.Address.BlockchainAddress).Distinct()
                    .Select(s => new ControllerAddressDto { BlockchainAddress = s }).ToList();

                return Result.Success(assetDto);
            }
        }

        public Result<ValidatorInfoDto> GetValidatorInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var validator = NewRepository<Validator>(uow)
                    .Get(v => v.BlockchainAddress == blockchainAddress && !v.IsDeleted)
                    .SingleOrDefault();

                if (validator is null)
                    return Result.Failure<ValidatorInfoDto>("Validator {0} does not exist.".F(blockchainAddress));

                var validatorDto = ValidatorInfoDto.FromDomainModel(validator);

                var stakeActionIds = eventRepo.GetAs(
                    e => e.Address.BlockchainAddress == blockchainAddress
                    && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                    && e.Amount > 0, e => e.TxActionId);

                validatorDto.Stakes = eventRepo
                    .Get(e => stakeActionIds.Contains(e.TxActionId) && e.Amount < 0, e => e.Address)
                    .Select(e => new StakeDto { StakerAddress = e.Address.BlockchainAddress, Amount = e.Amount.Value * -1 })
                    .ToList();

                return Result.Success(validatorDto);
            }
        }

        public Result<IEnumerable<TxInfoShortDto>> GetTxs(int limit, int page)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(_blockchainInfoRepositoryFactory.Create(uow).GetTxs(limit, page));
            }
        }

        public Result<IEnumerable<BlockInfoShortDto>> GetBlocks(int limit, int page)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(_blockchainInfoRepositoryFactory.Create(uow).GetBlocks(limit, page));
            }
        }
        public Result<IEnumerable<ValidatorInfoShortDto>> GetValidators()
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(NewRepository<Validator>(uow).GetAs(
                    v => !v.IsDeleted,
                    v => new ValidatorInfoShortDto {BlockchainAddress = v.BlockchainAddress, IsActive = v.IsActive}));
            }
        }

        public Result<object> Search(string hash)
        {
            using (var uow = NewUnitOfWork())
            {
                if (NewRepository<Address>(uow).Exists(a => a.BlockchainAddress == hash))
                    return ProcessSearchResult(GetAddressInfo(hash));
                else if (NewRepository<Account>(uow).Exists(a => a.Hash == hash))
                    return ProcessSearchResult(GetAccountInfo(hash));
                else if (NewRepository<Asset>(uow).Exists(a => a.Hash == hash))
                    return ProcessSearchResult(GetAssetInfo(hash));
                else if (NewRepository<Transaction>(uow).Exists(t => t.Hash == hash))
                    return ProcessSearchResult(GetTxInfo(hash));
                else if (NewRepository<Equivocation>(uow).Exists(e => e.EquivocationProofHash == hash))
                    return ProcessSearchResult(GetEquivocationInfo(hash));
                else
                {
                    if (long.TryParse(hash, out long number))
                    {
                        if(NewRepository<Block>(uow).Exists(a => a.BlockNumber == number))
                            return ProcessSearchResult(GetBlockInfo(number));
                    }
                }

                return Result.Failure<object>("Not found.");
            }
        }

        private Result<object> ProcessSearchResult<T>(Result<T> result)
        {
            if (result.Successful)
                return Result.Success((object)result.Data);
            else
                return Result.Failure<object>(result.Alerts);  
        } 
    }
}
