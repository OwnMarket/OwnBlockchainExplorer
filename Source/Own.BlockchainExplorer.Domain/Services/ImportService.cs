using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ImportService : DataService, IImportService
    {
        private readonly IActionService _actionService;

        public ImportService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IActionService actionService)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _actionService = actionService;
        }

        public Result<Address> ImportAddress(string blockchainAddress, long nonce, IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var address = addressRepo.Get(a => a.BlockchainAddress == blockchainAddress).SingleOrDefault();

            if (address == null)
            {
                address = new Address
                {
                    BlockchainAddress = blockchainAddress,
                    Nonce = nonce,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };
                addressRepo.Insert(address);
            }
            else
            {
                address.Nonce = nonce;
                addressRepo.Update(address);
            }

            uow.Commit();
            return Result.Success(address);
        }

        public Result<Block> ImportBlock(BlockDto blockDto, IUnitOfWork uow)
        {
            var blockRepo = NewRepository<Block>(uow);

            if (blockRepo.Exists(b => b.BlockNumber == blockDto.Number))
                return Result.Failure<Block>("Block {0} already exists.".F(blockDto.Number));

            var block = new Block
            {
                BlockNumber = blockDto.Number,
                Hash = blockDto.Hash,
                PreviousBlockHash = blockDto.PreviousHash,
                ConfigurationBlockNumber = blockDto.ConfigurationBlockNumber,
                Timestamp = blockDto.Timestamp,
                TxSetRoot = blockDto.TxSetRoot,
                TxResultSetRoot = blockDto.TxResultSetRoot,
                EquivocationProofsRoot = blockDto.EquivocationProofsRoot,
                EquivocationProofResultsRoot = blockDto.EquivocationProofResultsRoot,
                StateRoot = blockDto.StateRoot,
                StakingRewardsRoot = blockDto.StakingRewardsRoot,
                ConfigurationRoot = blockDto.ConfigurationRoot,
                Configuration = JsonConvert.SerializeObject(blockDto.Configuration),
                ConsensusRound = blockDto.ConsensusRound,
                Signatures = string.Join(';', blockDto.Signatures)
            };

            var previousBlockId = blockRepo
                .GetAs(b => b.Hash == blockDto.PreviousHash, b => b.BlockId)
                .SingleOrDefault();

            if (previousBlockId != default(long))
                block.PreviousBlockId = previousBlockId;

            block.ValidatorId = NewRepository<Validator>(uow)
                .GetAs(v => v.BlockchainAddress == blockDto.ProposerAddress, v => v.ValidatorId)
                .SingleOrDefault();

            blockRepo.Insert(block);
            uow.Commit();

            return Result.Success(block);
        }

        public Result<Tx> ImportTx(TxDto txDto, long timestamp, IUnitOfWork uow)
        {
            var txRepo = NewRepository<Tx>(uow);

            if (txRepo.Exists(t => t.Hash == txDto.TxHash))
                return Result.Failure<Tx>("TX {0} already exists.".F(txDto.TxHash));

            var tx =
                new Tx
                {
                    Hash = txDto.TxHash,
                    Nonce = txDto.Nonce,
                    Timestamp = timestamp,
                    DateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime,
                    ExpirationTime = txDto.ExpirationTime != 0
                        ? (DateTime?)DateTimeOffset.FromUnixTimeMilliseconds(txDto.ExpirationTime).UtcDateTime
                        : null,
                    ActionFee = txDto.ActionFee,
                    Status = txDto.Status,
                    ErrorMessage = txDto.ErrorCode,
                    FailedActionNumber = txDto.FailedActionNumber
                };

            txRepo.Insert(tx);
            uow.Commit();

            return Result.Success(tx);
        }

        public Result<Equivocation> ImportEquivocation(EquivocationDto equivocationDto, long blockId, IUnitOfWork uow)
        {
            var equivocationRepo = NewRepository<Equivocation>(uow);

            if (equivocationRepo.Exists(e => e.EquivocationProofHash == equivocationDto.EquivocationProofHash))
                return Result.Failure<Equivocation>(
                    "Equivocation {0} already exists.".F(equivocationDto.EquivocationProofHash));

            var equivocation =
                new Equivocation
                {
                    EquivocationProofHash = equivocationDto.EquivocationProofHash,
                    BlockNumber = equivocationDto.BlockNumber,
                    ConsensusRound = equivocationDto.ConsensusRound,
                    ConsensusStep = equivocationDto.ConsensusStep,
                    EquivocationValue1 = equivocationDto.EquivocationValue1,
                    EquivocationValue2 = equivocationDto.EquivocationValue2,
                    Signature1 = equivocationDto.Signature1,
                    Signature2 = equivocationDto.Signature2,
                    BlockId = blockId
                };

            equivocationRepo.Insert(equivocation);
            uow.Commit();

            return Result.Success(equivocation);
        }

        public Result<BlockchainEvent> ImportStakingRewardEvent(
            StakingRewardDto stakingRewardDto,
            long blockId,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);

            var blockchainEvent =
                new BlockchainEvent
                {
                    Amount = stakingRewardDto.Amount,
                    BlockId = blockId,
                    EventType = EventType.StakingReward.ToString()
                };

            var address = addressRepo
                .Get(a => a.BlockchainAddress == stakingRewardDto.StakerAddress)
                .SingleOrDefault();

            if (address is null)
                return Result.Failure<BlockchainEvent>("Address {0} does not exist.".F(stakingRewardDto.StakerAddress));

            blockchainEvent.AddressId = address.AddressId;
            UpdateValidatorBalance(address, stakingRewardDto.Amount);

            addressRepo.Update(address);
            NewRepository<BlockchainEvent>(uow).Insert(blockchainEvent);
            uow.Commit();

            return Result.Success(blockchainEvent);
        }

        public Result<BlockchainEvent> ImportValidatorRewardEvent(
            decimal reward,
            long blockId,
            string blockchainAddress,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);

            var blockchainEvent =
                new BlockchainEvent
                {
                    Amount = reward,
                    BlockId = blockId,
                    EventType = EventType.ValidatorReward.ToString()
                };

            var address = addressRepo
                .Get(a => a.BlockchainAddress == blockchainAddress)
                .SingleOrDefault();

            if (address is null)
                return Result.Failure<BlockchainEvent>("Address {0} does not exist.".F(blockchainAddress));

            UpdateValidatorBalance(address, reward, uow);

            blockchainEvent.AddressId = address.AddressId;

            addressRepo.Update(address);
            NewRepository<BlockchainEvent>(uow).Insert(blockchainEvent);
            uow.Commit();

            return Result.Success(blockchainEvent);
        }

        public Result<BlockchainEvent> ImportDepositTakenEvent(
            EquivocationDto equivocationDto,
            long blockId,
            long equivocationId,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);

            var depositTakenEvent = new BlockchainEvent
            {
                EventType = EventType.DepositTaken.ToString(),
                Amount = equivocationDto.DepositTaken * -1,
                EquivocationId = equivocationId,
                BlockId = blockId
            };

            var address = addressRepo
                .Get(a => a.BlockchainAddress == equivocationDto.ValidatorAddress)
                .SingleOrDefault();

            if (address is null)
                return Result.Failure<BlockchainEvent>(
                    "Address {0} does not exist.".F(equivocationDto.ValidatorAddress));

            depositTakenEvent.AddressId = address.AddressId;

            address.DepositBalance -= equivocationDto.DepositTaken;
            var newDepositAmount = Config.ValidatorDeposit - address.DepositBalance;

            var amountToDeduce = address.AvailableBalance > newDepositAmount
                ? newDepositAmount
                : address.AvailableBalance;

            address.DepositBalance += amountToDeduce;
            address.AvailableBalance -= amountToDeduce;

            addressRepo.Update(address);

            NewRepository<BlockchainEvent>(uow).Insert(depositTakenEvent);
            uow.Commit();

            return Result.Success(depositTakenEvent);
        }

        public Result<IEnumerable<BlockchainEvent>> ImportDepositGivenEvents(
            EquivocationDto equivocationDto,
            long blockId,
            long equivocationId,
            IUnitOfWork uow)
        {
            var eventRepo = NewRepository<BlockchainEvent>(uow);
            var addressRepo = NewRepository<Address>(uow);

            var events = new List<BlockchainEvent>();
            foreach (var depositDto in equivocationDto.DepositDistribution)
            {
                var depositGivenEvent = new BlockchainEvent
                {
                    EventType = EventType.DepositGiven.ToString(),
                    Amount = depositDto.Amount,
                    EquivocationId = equivocationId,
                    BlockId = blockId
                };

                var address = addressRepo
                    .Get(a => a.BlockchainAddress == depositDto.ValidatorAddress)
                    .SingleOrDefault();

                if (address is null)
                    return Result.Failure<IEnumerable<BlockchainEvent>>(
                        "Address {0} does not exist.".F(depositDto.ValidatorAddress));

                depositGivenEvent.AddressId = address.AddressId;
                events.Add(depositGivenEvent);

                address.AvailableBalance += depositDto.Amount;
                addressRepo.Update(address);
            }

            eventRepo.Insert(events);
            uow.Commit();

            return Result.Success(events.AsEnumerable());
        }

        public Result<TxAction> ImportAction(ActionDto actionDto, int actionNumber, IUnitOfWork uow)
        {
            var action = new TxAction
            {
                ActionNumber = actionNumber,
                ActionType = actionDto.ActionType,
                ActionData = JsonConvert.SerializeObject(actionDto.ActionData),
            };

            NewRepository<TxAction>(uow).Insert(action);
            uow.Commit();

            return Result.Success(action);
        }

        public Result<IEnumerable<BlockchainEvent>> ImportEvents(
            TxAction action,
            Address senderAddress,
            long blockId,
            Tx tx,
            JObject actionDataObj,
            IUnitOfWork uow)
        {
            var senderEvent = new BlockchainEvent
            {
                AddressId = senderAddress.AddressId,
                TxActionId = action.TxActionId,
                BlockId = blockId,
                Fee = tx.ActionFee,
                Amount = 0,
                TxId = tx.TxId,
                EventType = EventType.Action.ToString()
            };
            var events = new List<BlockchainEvent> { senderEvent };
            senderAddress.AvailableBalance -= tx.ActionFee;

            if (tx.Status == TxStatus.Success.ToString())
            {
                Result<List<BlockchainEvent>> result;

                switch (action.ActionType.ToEnum<ActionType>())
                {
                    case ActionType.TransferChx:
                        result = _actionService.TransferChx(
                            senderEvent,
                            actionDataObj.ToObject<TransferChxData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.DelegateStake:
                        result = _actionService.DelegateStake(
                            senderEvent,
                            actionDataObj.ToObject<DelegateStakeData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.ConfigureValidator:
                        result = _actionService.ConfigureValidator(
                            actionDataObj.ToObject<ConfigureValidatorData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.RemoveValidator:
                        result = _actionService.RemoveValidator(senderEvent, senderAddress, uow);
                        break;
                    case ActionType.SetAssetCode:
                        result = _actionService.SetAssetCode(
                            senderEvent,
                            actionDataObj.ToObject<SetAssetCodeData>(),
                            uow);
                        break;
                    case ActionType.SetAssetController:
                        result = _actionService.SetAssetController(
                            senderEvent,
                            actionDataObj.ToObject<SetAssetControllerData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.SetAccountController:
                        result = _actionService.SetAccountController(
                            senderEvent,
                            actionDataObj.ToObject<SetAccountControllerData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.TransferAsset:
                        result = _actionService.TransferAsset(
                            senderEvent,
                            actionDataObj.ToObject<TransferAssetData>(),
                            uow);
                        break;
                    case ActionType.CreateAssetEmission:
                        result = _actionService.CreateAssetEmission(
                            senderEvent,
                            actionDataObj.ToObject<CreateAssetEmissionData>(),
                            uow);
                        break;
                    case ActionType.CreateAsset:
                        result = _actionService.CreateAsset(senderEvent, senderAddress, action, uow);
                        break;
                    case ActionType.CreateAccount:
                        result = _actionService.CreateAccount(senderEvent, senderAddress, action, uow);
                        break;
                    case ActionType.SubmitVote:
                        result = _actionService.SubmitVote(senderEvent, actionDataObj.ToObject<SubmitVoteData>(), uow);
                        break;
                    case ActionType.SubmitVoteWeight:
                        result = _actionService.SubmitVoteWeight(
                            senderEvent,
                            actionDataObj.ToObject<SubmitVoteWeightData>(),
                            uow);
                        break;
                    case ActionType.SetAccountEligibility:
                        result = _actionService.SetAccountEligibility(
                            senderEvent,
                            actionDataObj.ToObject<SetAccountEligibilityData>(),
                            uow);
                        break;
                    case ActionType.SetAssetEligibility:
                        result = _actionService.SetAssetEligibility(
                            senderEvent,
                            actionDataObj.ToObject<SetAssetEligibilityData>(),
                            uow);
                        break;
                    case ActionType.ChangeKycControllerAddress:
                        result = _actionService.ChangeKycControllerAddress(
                            senderEvent,
                            actionDataObj.ToObject<ChangeKycControllerAddressData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.AddKycProvider:
                        result = _actionService.AddKycProvider(
                            senderEvent,
                            actionDataObj.ToObject<AddKycProviderData>(),
                            senderAddress,
                            uow);
                        break;
                    case ActionType.RemoveKycProvider:
                        result = _actionService.RemoveKycProvider(
                            senderEvent,
                            actionDataObj.ToObject<RemoveKycProviderData>(),
                            senderAddress,
                            uow);
                        break;
                    default:
                        result = Result.Failure<List<BlockchainEvent>>("Unsupported action type.");
                        break;
                }

                if (result.Failed)
                    return Result.Failure<IEnumerable<BlockchainEvent>>(result.Alerts);
                else
                    events.AddRange(result.Data);
            }

            NewRepository<BlockchainEvent>(uow).Insert(events);
            NewRepository<Address>(uow).Update(senderAddress);

            uow.Commit();
            return Result.Success(events.AsEnumerable());
        }

        private void UpdateValidatorBalance(Address validatorAddress, decimal amount)
        {
            if (validatorAddress.DepositBalance < Config.ValidatorDeposit)
            {
                var missingDepositBalance = Config.ValidatorDeposit - validatorAddress.DepositBalance;
                if (amount <= missingDepositBalance)
                    validatorAddress.DepositBalance += amount;
                else
                {
                    validatorAddress.DepositBalance += missingDepositBalance;
                    validatorAddress.AvailableBalance += amount - missingDepositBalance;
                }
            }
            else
            {
                validatorAddress.AvailableBalance += amount;
            }
        }
    }
}
