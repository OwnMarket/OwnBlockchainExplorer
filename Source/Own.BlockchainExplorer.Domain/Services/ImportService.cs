using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ImportService : DataService, IImportService
    {
        private readonly IBlockchainClient _blockchainClient;
        private readonly IActionService _actionService;

        public ImportService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IBlockchainClient blockchainClient,
            IActionService actionService)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainClient = blockchainClient;
            _actionService = actionService;
        }

        public Result<Address> ImportAddress(string blockchainAddress, long nonce)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var addressRepo = NewRepository<Address>(uow);
                var address = addressRepo.Get(a => a.BlockchainAddress == blockchainAddress).SingleOrDefault();
                var newAddress = address == null;

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
        }

        public Result<Block> ImportBlock(BlockDto blockDto)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var blockRepo = NewRepository<Block>(uow);

                if (blockRepo.Exists(b => b.BlockNumber == blockDto.Number))
                    return Result.Failure<Block>("Block {0} already exists.".F(blockDto.Number));

                var block = new Block
                {
                    BlockNumber = blockDto.Number,
                    Hash = blockDto.Hash,
                    PreviousBlockHash = blockDto.PreviousBlockHash,
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
                    .GetAs(b => b.Hash == blockDto.PreviousBlockHash, b => b.BlockId)
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
        }

        public Result<Transaction> ImportTx(TxDto txDto, long timestamp)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var txRepo = NewRepository<Transaction>(uow);

                if (txRepo.Exists(t => t.Hash == txDto.TxHash))
                    return Result.Failure<Transaction>("Tx {0} already exists.".F(txDto.TxHash));

                var tx =
                    new Transaction
                    {
                        Hash = txDto.TxHash,
                        Nonce = txDto.Nonce,
                        Timestamp = timestamp,
                        ActionFee = txDto.ActionFee,
                        Status = txDto.Status,
                        ErrorMessage = txDto.ErrorCode,
                        FailedActionNumber = txDto.FailedActionNumber
                    };

                txRepo.Insert(tx);
                uow.Commit();

                return Result.Success(tx);
            }
        }

        public Result<Equivocation> ImportEquivocation(EquivocationDto equivocationDto, long blockId)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
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
        }

        public Result<BlockchainEvent> ImportStakingRewardEvent(StakingRewardDto stakingRewardDto, long blockId)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);

                var blockchainEvent =
                    new BlockchainEvent
                    {
                        Amount = stakingRewardDto.Amount,
                        BlockId = blockId,
                        EventType = EventType.StakingReward.ToString()
                    };

                var addressId = NewRepository<Address>(uow)
                    .GetAs(a => a.BlockchainAddress == stakingRewardDto.StakerAddress, a => a.AddressId)
                    .SingleOrDefault();

                if (addressId == default(long))
                    return Result.Failure<BlockchainEvent>("Address {0} does not exist.".F(stakingRewardDto.StakerAddress));

                blockchainEvent.AddressId = addressId;

                eventRepo.Insert(blockchainEvent);
                uow.Commit();

                return Result.Success(blockchainEvent);
            }
        }

        public Result<BlockchainEvent> ImportValidatorRewardEvent(
            decimal reward, 
            long blockId, 
            string blockchainAddress)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);

                var blockchainEvent =
                    new BlockchainEvent
                    {
                        Amount = reward,
                        BlockId = blockId,
                        EventType = EventType.ValidatorReward.ToString()
                    };

                var addressId = NewRepository<Address>(uow)
                    .GetAs(a => a.BlockchainAddress == blockchainAddress, a => a.AddressId)
                    .SingleOrDefault();

                if (addressId == default(long))
                    return Result.Failure<BlockchainEvent>("Address {0} does not exist.".F(blockchainAddress));

                blockchainEvent.AddressId = addressId;

                eventRepo.Insert(blockchainEvent);
                uow.Commit();

                return Result.Success(blockchainEvent);
            }
        }

        public Result<BlockchainEvent> ImportDepositTakenEvent(
            EquivocationDto equivocationDto, 
            long blockId, 
            long equivocationId)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);

                var depositTakenEvent = new BlockchainEvent
                {
                    EventType = EventType.DepositTaken.ToString(),
                    Amount = equivocationDto.DepositTaken * -1,
                    EquivocationId = equivocationId,
                    BlockId = blockId
                };
                var addressId = NewRepository<Address>(uow)
                    .GetAs(a => a.BlockchainAddress == equivocationDto.ValidatorAddress, a => a.AddressId)
                    .SingleOrDefault();

                if (addressId == default(long))
                    return Result.Failure<BlockchainEvent>("Address {0} does not exist.".F(equivocationDto.ValidatorAddress));
                depositTakenEvent.AddressId = addressId;

                eventRepo.Insert(depositTakenEvent);
                uow.Commit();

                return Result.Success(depositTakenEvent);
            }
        }

        public Result<IEnumerable<BlockchainEvent>> ImportDepositGivenEvents(
            EquivocationDto equivocationDto,
            long blockId,
            long equivocationId)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
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

                    var addressId = addressRepo
                        .GetAs(a => a.BlockchainAddress == depositDto.ValidatorAddress, a => a.AddressId)
                        .SingleOrDefault();

                    if (addressId == default(long))
                        return Result.Failure<IEnumerable<BlockchainEvent>>("Address {0} does not exist.".F(depositDto.ValidatorAddress));
                    depositGivenEvent.AddressId = addressId;
                }

                eventRepo.Insert(events);
                uow.Commit();

                return Result.Success(events.AsEnumerable());
            }
        }

        public Result<TxAction> ImportAction(ActionDto actionDto, int actionNumber)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
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
        }

        public Result<IEnumerable<BlockchainEvent>> ImportEvents(
            TxAction action,
            Address senderAddress,
            Block block,
            Transaction tx,
            JObject actionDataObj)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                List<BlockchainEvent> events = new List<BlockchainEvent>
                {
                    new BlockchainEvent
                    {
                        AddressId = senderAddress.AddressId,
                        TxActionId = action.TxActionId,
                        BlockId = block.BlockId,
                        Fee = tx.ActionFee,
                        Amount = 0,
                        TransactionId = tx.TransactionId,
                        EventType = EventType.Action.ToString()
                    }
                };
                senderAddress.AvailableBalance -= tx.ActionFee;

                if (tx.Status == TxStatus.Success.ToString())
                {
                    Result result = Result.Success();
                    switch (action.ActionType.ToEnum<ActionType>())
                    {
                        case ActionType.TransferChx:
                            result = _actionService.TransferChx(
                                events, 
                                actionDataObj.ToObject<TransferChxData>(), 
                                uow, 
                                senderAddress);
                            break;
                        case ActionType.DelegateStake:
                            result = _actionService.DelegateStake(
                                events,
                                actionDataObj.ToObject<DelegateStakeData>(),
                                uow,
                                senderAddress);
                            break;
                        case ActionType.ConfigureValidator:
                            result = _actionService.ConfigureValidator(
                                events,
                                actionDataObj.ToObject<ConfigureValidatorData>(),
                                uow,
                                senderAddress);
                            break;
                        case ActionType.RemoveValidator:
                            result = _actionService.RemoveValidator(events, uow, senderAddress);
                            break;
                        case ActionType.SetAssetCode:
                            result = _actionService.SetAssetCode(
                                events, 
                                actionDataObj.ToObject<SetAssetCodeData>(), 
                                uow);
                            break;
                        case ActionType.SetAssetController:
                            result = _actionService.SetAssetController(
                                events,
                                actionDataObj.ToObject<SetAssetControllerData>(),
                                uow);
                            break;
                        case ActionType.SetAccountController:
                            result = _actionService.SetAccountController(
                                events,
                                actionDataObj.ToObject<SetAccountControllerData>(),
                                uow);
                            break;
                        case ActionType.TransferAsset:
                            result = _actionService.TransferAsset(
                                events, 
                                actionDataObj.ToObject<TransferAssetData>(), 
                                uow);
                            break;
                        case ActionType.CreateAssetEmission:
                            result = _actionService.CreateAssetEmission(
                                events,
                                actionDataObj.ToObject<CreateAssetEmissionData>(),
                                uow);
                            break;
                        case ActionType.CreateAsset:
                            result = _actionService.CreateAsset(events, uow, senderAddress, action);
                            break;
                        case ActionType.CreateAccount:
                            result = _actionService.CreateAccount(events, uow, senderAddress, action);
                            break;
                        case ActionType.SubmitVote:
                            result = _actionService.SubmitVote(events, actionDataObj.ToObject<SubmitVoteData>(), uow);
                            break;
                        case ActionType.SubmitVoteWeight:
                            result = _actionService.SubmitVoteWeight(
                                events,
                                actionDataObj.ToObject<SubmitVoteWeightData>(),
                                uow);
                            break;
                        case ActionType.SetAccountEligibility:
                            result = _actionService.SetAccountEligibility(
                                events,
                                actionDataObj.ToObject<SetAccountEligibilityData>(),
                                uow);
                            break;
                        case ActionType.SetAssetEligibility:
                            result = _actionService.SetAssetEligibility(
                                events, 
                                actionDataObj.ToObject<SetAssetEligibilityData>(), 
                                uow);
                            break;
                        case ActionType.ChangeKycControllerAddress:
                            result = _actionService.ChangeKycControllerAddress(
                                events,
                                actionDataObj.ToObject<ChangeKycControllerAddressData>(),
                                uow);
                            break;
                        case ActionType.AddKycProvider:
                            result = _actionService.AddKycProvider(
                                events, 
                                actionDataObj.ToObject<AddKycProviderData>(), 
                                uow);
                            break;
                        case ActionType.RemoveKycProvider:
                            result = _actionService.RemoveKycProvider(
                                events, 
                                actionDataObj.ToObject<RemoveKycProviderData>(), 
                                uow);
                            break;
                        default:
                            result = Result.Failure("Unsupported action type.");
                            break;
                    }

                    if (result.Failed)
                        return Result.Failure<IEnumerable<BlockchainEvent>>(result.Alerts);
                }

                NewRepository<BlockchainEvent>(uow).Insert(events);
                NewRepository<Address>(uow).Update(senderAddress);

                uow.Commit();
                return Result.Success(events.AsEnumerable());
            }
        }
    }
}
