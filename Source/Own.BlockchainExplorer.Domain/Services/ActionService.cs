using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ActionService : DataService, IActionService
    {
        private readonly IBlockchainCryptoProvider _blockchainCryptoProvider;

        public ActionService(
            IBlockchainCryptoProvider cryptoProvider,
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainCryptoProvider = cryptoProvider;
        }

        public Result<List<BlockchainEvent>> TransferChx(
            BlockchainEvent senderEvent,
            TransferChxData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);

            senderEvent.Amount = actionData.Amount * -1;
            senderAddress.AvailableBalance -= actionData.Amount;

            var sameAddress = senderAddress.BlockchainAddress == actionData.RecipientAddress;

            var recipientAddress = sameAddress
                ? senderAddress
                : addressRepo.Get(a => a.BlockchainAddress == actionData.RecipientAddress).SingleOrDefault();

            var newAddress = recipientAddress is null;
            if (newAddress)
            {
                recipientAddress = new Address
                {
                    BlockchainAddress = actionData.RecipientAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };
            }

            UpdateBalance(recipientAddress, actionData.Amount, uow);

            var events = new List<BlockchainEvent>
            {
                new BlockchainEvent
                {
                    Address = recipientAddress,
                    TxActionId = senderEvent.TxActionId,
                    BlockId = senderEvent.BlockId,
                    Amount = actionData.Amount,
                    TxId = senderEvent.TxId,
                    EventType = EventType.Action.ToString()
                }
            };

            if (!sameAddress)
            {
                if (newAddress)
                    addressRepo.Insert(recipientAddress);
                else
                    addressRepo.Update(recipientAddress);
            }

            return Result.Success(events);
        }

        public Result<List<BlockchainEvent>> DelegateStake(
            BlockchainEvent senderEvent,
            DelegateStakeData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);

            senderEvent.Amount = actionData.Amount * -1;
            senderAddress.StakedBalance += actionData.Amount;
            senderAddress.AvailableBalance -= actionData.Amount;

            var sameAddress = senderAddress.BlockchainAddress == actionData.ValidatorAddress;

            var validatorAddress = sameAddress
                ? senderAddress
                : addressRepo.Get(a => a.BlockchainAddress == actionData.ValidatorAddress).SingleOrDefault();

            var newAddress = validatorAddress is null;
            if (newAddress)
            {
                validatorAddress = new Address
                {
                    BlockchainAddress = actionData.ValidatorAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };
            }

            var events = new List<BlockchainEvent>()
            {
                new BlockchainEvent
                {
                    Address = validatorAddress,
                    TxActionId = senderEvent.TxActionId,
                    BlockId = senderEvent.BlockId,
                    Amount = actionData.Amount,
                    TxId = senderEvent.TxId,
                    EventType = EventType.Action.ToString()
                }
            };

            if (!sameAddress)
            {
                if (newAddress)
                    addressRepo.Insert(validatorAddress);
                else
                    addressRepo.Update(validatorAddress);
            }

            return Result.Success(events);
        }

        public Result<List<BlockchainEvent>> ConfigureValidator(
            ConfigureValidatorData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var validatorRepo = NewRepository<Validator>(uow);

            var validator = validatorRepo.Get(v => v.BlockchainAddress == senderAddress.BlockchainAddress).SingleOrDefault();
            if (validator is null)
            {
                validatorRepo.Insert(new Validator()
                {
                    BlockchainAddress = senderAddress.BlockchainAddress,
                    NetworkAddress = actionData.NetworkAddress,
                    SharedRewardPercent = actionData.SharedRewardPercent,
                    IsActive = false,
                    IsDeleted = false
                });
            }
            else
            {
                validator.NetworkAddress = actionData.NetworkAddress;
                if (!validator.GeoLocation.IsNullOrEmpty())
                {
                    var geoLocation = JsonConvert.DeserializeObject<ValidatorGeoInfoDto>(validator.GeoLocation);
                    geoLocation.NetworkAddress = actionData.NetworkAddress;
                    validator.GeoLocation = JsonConvert.SerializeObject(geoLocation);
                }

                validator.SharedRewardPercent = actionData.SharedRewardPercent;
                validator.IsDeleted = false;
                validatorRepo.Update(validator);
            }

            var depositAmount = Config.ValidatorDeposit - senderAddress.DepositBalance;
            senderAddress.DepositBalance += depositAmount;
            senderAddress.AvailableBalance -= depositAmount;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> RemoveValidator(
            BlockchainEvent senderEvent, 
            Address senderAddress, 
            IUnitOfWork uow, 
            bool deleteValidator = true)
        {
            var validatorRepo = NewRepository<Validator>(uow);
            var eventRepo = NewRepository<BlockchainEvent>(uow);
            var addressRepo = NewRepository<Address>(uow);

            var validator = validatorRepo
                .Get(v => v.BlockchainAddress == senderAddress.BlockchainAddress)
                .SingleOrDefault();
            if (validator is null)
                return Result.Failure<List<BlockchainEvent>>("Address {0} is not a validator.".F(senderAddress.BlockchainAddress));

            if (deleteValidator)
            {
                validator.IsDeleted = true;
                validatorRepo.Update(validator);

                senderAddress.AvailableBalance += senderAddress.DepositBalance;
                senderAddress.DepositBalance = 0;
            }

            var delegateStakeIds = eventRepo.GetAs(
                e => e.EventType == EventType.Action.ToString()
                && e.AddressId == senderAddress.AddressId
                && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                && e.Fee == null,
                e => e.TxActionId);

            var delegateStakeEvents = eventRepo.Get(
                e => delegateStakeIds.Contains(e.TxActionId) && e.Fee != null,
                e => e.Address).GroupBy(e => e.Address);

            var events = new List<BlockchainEvent>();
            // Stakers StakeReturned events
            foreach (var group in delegateStakeEvents)
            {
                var sameAddress = senderAddress.AddressId == group.Key.AddressId;

                var address = sameAddress ? senderAddress : group.Key;
                var stakedAmount = group.Sum(e => e.Amount ?? 0) * -1;

                events.Add(new BlockchainEvent {
                    AddressId = address.AddressId,
                    Amount = stakedAmount,
                    BlockId = senderEvent.BlockId,
                    TxId = senderEvent.TxId,
                    TxActionId = senderEvent.TxActionId,
                    EventType = EventType.StakeReturned.ToString(),
                    GroupingId = senderEvent.GroupingId
                });

                address.StakedBalance -= stakedAmount;
                address.AvailableBalance += stakedAmount;

                if (!sameAddress)
                    addressRepo.Update(address);
            }

            // Validator StakeReturned event
            events.Add(new BlockchainEvent
            {
                AddressId = senderAddress.AddressId,
                Amount = delegateStakeEvents.Select(g => g.Sum(e => e.Amount) ?? 0).Sum(),
                BlockId = senderEvent.BlockId,
                TxId = senderEvent.TxId,
                TxActionId = senderEvent.TxActionId,
                EventType = EventType.StakeReturned.ToString(),
                GroupingId = senderEvent.GroupingId
            });

            return Result.Success(events);
        }

        public Result<List<BlockchainEvent>> SetAssetCode(BlockchainEvent senderEvent, SetAssetCodeData actionData, IUnitOfWork uow)
        {
            var assetRepo = NewRepository<Asset>(uow);

            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));
            asset.AssetCode = actionData.AssetCode;
            assetRepo.Update(asset);

            senderEvent.AssetId = asset.AssetId;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> SetAssetController(
            BlockchainEvent senderEvent,
            SetAssetControllerData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var assetRepo = NewRepository<Asset>(uow);

            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>(
                    "Asset {0} does not exist.".F(actionData.AssetHash));
            asset.ControllerAddress = actionData.ControllerAddress;

            senderEvent.AssetId = asset.AssetId;

            var sameAddress = senderAddress.BlockchainAddress == actionData.ControllerAddress;

            var controllerAddress = sameAddress
                ? senderAddress
                : addressRepo
                    .Get(a => a.BlockchainAddress == actionData.ControllerAddress)
                    .SingleOrDefault();

            if (controllerAddress is null)
            {
                controllerAddress = new Address
                {
                    BlockchainAddress = actionData.ControllerAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };

                addressRepo.Insert(controllerAddress);
            }

            var events = new List<BlockchainEvent>()
            {
                new BlockchainEvent
                {
                    Address = controllerAddress,
                    TxActionId = senderEvent.TxActionId,
                    BlockId = senderEvent.BlockId,
                    Fee = 0,
                    Amount = 0,
                    TxId = senderEvent.TxId,
                    EventType = EventType.Action.ToString(),
                    AssetId = asset.AssetId
                }
            };

            assetRepo.Update(asset);

            return Result.Success(events);
        }

        public Result<List<BlockchainEvent>> SetAccountController(
            BlockchainEvent senderEvent,
            SetAccountControllerData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var accountRepo = NewRepository<Account>(uow);

            var account = accountRepo.Get(a => a.Hash == actionData.AccountHash).SingleOrDefault();
            if (account is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.AccountHash));
            account.ControllerAddress = actionData.ControllerAddress;

            senderEvent.AccountId = account.AccountId;
            var sameAddress = senderAddress.BlockchainAddress == actionData.ControllerAddress;

            var controllerAddress = sameAddress
                ? senderAddress
                : addressRepo
                .Get(a => a.BlockchainAddress == actionData.ControllerAddress)
                .SingleOrDefault();
            if (controllerAddress is null)
            {
                controllerAddress = new Address
                {
                    BlockchainAddress = actionData.ControllerAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };

                addressRepo.Insert(controllerAddress);
            }

            var events = new List<BlockchainEvent>()
            {
                new BlockchainEvent
                {
                    Address = controllerAddress,
                    TxActionId = senderEvent.TxActionId,
                    BlockId = senderEvent.BlockId,
                    Fee = 0,
                    Amount = 0,
                    TxId = senderEvent.TxId,
                    EventType = EventType.Action.ToString(),
                    AccountId = account.AccountId
                }
            };

            accountRepo.Update(account);

            return Result.Success(events);
        }

        public Result<List<BlockchainEvent>> TransferAsset(
            BlockchainEvent senderEvent,
            TransferAssetData actionData,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var accountRepo = NewRepository<Account>(uow);
            var assetRepo = NewRepository<Asset>(uow);
            var holdingRepo = NewRepository<Holding>(uow);

            var sameAccount = actionData.FromAccountHash == actionData.ToAccountHash;

            var fromAccount = accountRepo.Get(a => a.Hash == actionData.FromAccountHash).SingleOrDefault();
            if (fromAccount is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.FromAccountHash));

            var toAccount = sameAccount
                ? fromAccount
                : accountRepo.Get(a => a.Hash == actionData.ToAccountHash).SingleOrDefault();

            if (toAccount is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.ToAccountHash));

            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            var fromHolding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == fromAccount.AccountId)
                .SingleOrDefault();
            if (fromHolding is null || fromHolding.Balance < actionData.Amount)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not sufficient holding.".F(actionData.FromAccountHash));

            var toHolding = sameAccount
                ? fromHolding
                : holdingRepo
                    .Get(h => h.AssetId == asset.AssetId && h.AccountId == toAccount.AccountId)
                    .SingleOrDefault();
            var isNewHolding = toHolding is null;

            if (isNewHolding)
            {
                toHolding = new Holding
                {
                    AssetHash = asset.Hash,
                    AssetId = asset.AssetId,
                    AccountHash = toAccount.Hash,
                    AccountId = toAccount.AccountId,
                    Balance = 0
                };
            }

            fromHolding.Balance -= actionData.Amount;
            toHolding.Balance += actionData.Amount;

            senderEvent.AccountId = fromAccount.AccountId;
            senderEvent.AssetId = asset.AssetId;
            senderEvent.Amount = actionData.Amount * -1;

            var toControllerAddress = addressRepo
                .Get(a => a.BlockchainAddress == toAccount.ControllerAddress)
                .SingleOrDefault();

            if (toControllerAddress is null)
            {
                toControllerAddress = new Address
                {
                    BlockchainAddress = toAccount.ControllerAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };
            }

            var events = new List<BlockchainEvent>()
            {
                new BlockchainEvent
                {
                    Address = toControllerAddress,
                    TxActionId = senderEvent.TxActionId,
                    BlockId = senderEvent.BlockId,
                    Fee = 0,
                    Amount = actionData.Amount,
                    TxId = senderEvent.TxId,
                    EventType = EventType.Action.ToString(),
                    AccountId = toAccount.AccountId,
                    AssetId = asset.AssetId
                }
            };

            holdingRepo.Update(fromHolding);
            if (isNewHolding)
                holdingRepo.Insert(toHolding);
            else
                holdingRepo.Update(toHolding);

            return Result.Success(events);
        }

        public Result<List<BlockchainEvent>> CreateAssetEmission(
            BlockchainEvent senderEvent,
            CreateAssetEmissionData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.EmissionAccountHash)
                .SingleOrDefault();
            if (account is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.EmissionAccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            senderEvent.AccountId = account.AccountId;
            senderEvent.AssetId = asset.AssetId;
            senderEvent.Amount = actionData.Amount;

            var holdingRepo = NewRepository<Holding>(uow);
            var holding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == account.AccountId)
                .SingleOrDefault();

            var isNewHolding = holding is null;
            if (isNewHolding)
            {
                holding = new Holding
                {
                    AssetHash = asset.Hash,
                    AssetId = asset.AssetId,
                    AccountHash = account.Hash,
                    AccountId = account.AccountId
                };
            }
            holding.Balance += actionData.Amount;

            if (isNewHolding)
                holdingRepo.Insert(holding);
            else
                holdingRepo.Update(holding);

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> CreateAsset(
            BlockchainEvent senderEvent,
            Address senderAddress,
            TxAction action,
            IUnitOfWork uow)
        {
            var asset = new Asset
            {
                Hash = _blockchainCryptoProvider.DeriveHash(
                    senderAddress.BlockchainAddress,
                    senderAddress.Nonce,
                    (short)action.ActionNumber),
                ControllerAddress = senderAddress.BlockchainAddress,
                IsEligibilityRequired = false
            };

            NewRepository<Asset>(uow).Insert(asset);
            senderEvent.Asset = asset;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> CreateAccount(
            BlockchainEvent senderEvent,
            Address senderAddress,
            TxAction action,
            IUnitOfWork uow)
        {
            var account = new Account
            {
                Hash = _blockchainCryptoProvider.DeriveHash(
                    senderAddress.BlockchainAddress,
                    senderAddress.Nonce,
                    (short)action.ActionNumber),
                ControllerAddress = senderAddress.BlockchainAddress
            };

            NewRepository<Account>(uow).Insert(account);
            senderEvent.Account = account;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> SubmitVote(
            BlockchainEvent senderEvent,
            SubmitVoteData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            senderEvent.AccountId = account.AccountId;
            senderEvent.AssetId = asset.AssetId;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> SubmitVoteWeight(
            BlockchainEvent senderEvent,
            SubmitVoteWeightData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            senderEvent.AccountId = account.AccountId;
            senderEvent.AssetId = asset.AssetId;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> SetAccountEligibility(
            BlockchainEvent senderEvent,
            SetAccountEligibilityData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            var holdingRepo = NewRepository<Holding>(uow);
            var holding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == account.AccountId)
                .SingleOrDefault();

            var isNewHolding = holding is null;
            if (isNewHolding)
            {
                holding = new Holding
                {
                    AssetHash = asset.Hash,
                    AssetId = asset.AssetId,
                    AccountHash = account.Hash,
                    AccountId = account.AccountId
                };
            }

            holding.IsPrimaryEligible = actionData.IsPrimaryEligible;
            holding.IsSecondaryEligible = actionData.IsSecondaryEligible;

            senderEvent.AccountId = account.AccountId;
            senderEvent.AssetId = asset.AssetId;

            if (isNewHolding)
                holdingRepo.Insert(holding);
            else
                holdingRepo.Update(holding);

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> SetAssetEligibility(
            BlockchainEvent senderEvent,
            SetAssetEligibilityData actionData,
            IUnitOfWork uow)
        {
            var assetRepo = NewRepository<Asset>(uow);
            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            asset.IsEligibilityRequired = actionData.IsEligibilityRequired;
            assetRepo.Update(asset);

            senderEvent.AssetId = asset.AssetId;

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> ChangeKycControllerAddress(
            BlockchainEvent senderEvent,
            ChangeKycControllerAddressData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account is null)
                return Result.Failure<List<BlockchainEvent>>("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            var holdingRepo = NewRepository<Holding>(uow);
            var holding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == account.AccountId)
                .SingleOrDefault();
            var isNewHolding = holding is null;

            if (isNewHolding)
            {
                holding = new Holding()
                {
                    AssetHash = asset.Hash,
                    AssetId = asset.AssetId,
                    AccountHash = account.Hash,
                    AccountId = account.AccountId
                };
            }

            holding.KycControllerAddress = actionData.KycControllerAddress;

            senderEvent.AccountId = account.AccountId;
            senderEvent.AssetId = asset.AssetId;

            var sameAddress = senderAddress.BlockchainAddress == actionData.KycControllerAddress;

            var kycControllerAddress = sameAddress
                ? senderAddress
                : NewRepository<Address>(uow)
                    .Get(a => a.BlockchainAddress == actionData.KycControllerAddress)
                    .SingleOrDefault();


            var newAddress = kycControllerAddress is null;
            if (newAddress)
            {
                kycControllerAddress = new Address
                {
                    BlockchainAddress = actionData.KycControllerAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };

                NewRepository<Address>(uow).Insert(kycControllerAddress);
            }

            if (isNewHolding)
                holdingRepo.Insert(holding);
            else
                holdingRepo.Update(holding);

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> AddKycProvider(
            BlockchainEvent senderEvent,
            AddKycProviderData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            senderEvent.AssetId = asset.AssetId;

            var sameAddress = senderAddress.BlockchainAddress == actionData.ProviderAddress;
            var providerAddress = sameAddress
                ? senderAddress
                : NewRepository<Address>(uow)
                    .Get(a => a.BlockchainAddress == actionData.ProviderAddress)
                    .SingleOrDefault();

            if (providerAddress is null)
            {
                providerAddress = new Address
                {
                    BlockchainAddress = actionData.ProviderAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };

                NewRepository<Address>(uow).Insert(providerAddress);
            }

            return Result.Success(new List<BlockchainEvent>());
        }

        public Result<List<BlockchainEvent>> RemoveKycProvider(
            BlockchainEvent senderEvent,
            RemoveKycProviderData actionData,
            Address senderAddress,
            IUnitOfWork uow)
        {
            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset is null)
                return Result.Failure<List<BlockchainEvent>>("Asset {0} does not exist.".F(actionData.AssetHash));

            senderEvent.AssetId = asset.AssetId;

            var sameAddress = senderAddress.BlockchainAddress == actionData.ProviderAddress;
            var providerAddress = sameAddress
                ? senderAddress
                : NewRepository<Address>(uow)
                    .Get(a => a.BlockchainAddress == actionData.ProviderAddress)
                    .SingleOrDefault();

            if (providerAddress is null)
            {
                providerAddress = new Address
                {
                    BlockchainAddress = actionData.ProviderAddress,
                    Nonce = 0,
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0
                };

                NewRepository<Address>(uow).Insert(providerAddress);
            }

            return Result.Success(new List<BlockchainEvent>());
        }

        private void UpdateBalance(Address recipientAddress, decimal amount, IUnitOfWork uow)
        {
            var recipientIsValidator = NewRepository<Validator>(uow)
                .Exists(v =>
                    v.BlockchainAddress == recipientAddress.BlockchainAddress
                    && !v.IsDeleted);

            if (recipientIsValidator && recipientAddress.DepositBalance < Config.ValidatorDeposit)
            {
                var missingDepositBalance = Config.ValidatorDeposit - recipientAddress.DepositBalance;
                if (amount <= missingDepositBalance)
                    recipientAddress.DepositBalance += amount;
                else
                {
                    recipientAddress.DepositBalance += missingDepositBalance;
                    recipientAddress.AvailableBalance += amount - missingDepositBalance;
                }
            }
            else
            {
                recipientAddress.AvailableBalance += amount;
            }
        }
    }
}
