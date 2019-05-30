using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ActionService : DataService, IActionService
    {
        private readonly IBlockchainCryptoProvider _blockchainCryptoProvider;

        public ActionService(IBlockchainCryptoProvider cryptoProvider, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainCryptoProvider = cryptoProvider;
        }

        public Result TransferChx(
            List<BlockchainEvent> events, 
            TransferChxData actionData, 
            IUnitOfWork uow, 
            Address senderAddress)
        {
            var firstEvent = events.First();

            var addressRepo = NewRepository<Address>(uow);

            firstEvent.Amount = actionData.Amount * -1;
            senderAddress.AvailableBalance -= actionData.Amount;

            var recipientAddress = addressRepo.Get(a => a.BlockchainAddress == actionData.RecipientAddress).SingleOrDefault();
            var newAddress = recipientAddress == null;

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
            recipientAddress.AvailableBalance += actionData.Amount;

            events.Add(new BlockchainEvent
            {
                Address = recipientAddress,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Amount = actionData.Amount,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString()
            });

            if (newAddress)
                addressRepo.Insert(recipientAddress);
            else 
                addressRepo.Update(recipientAddress);

            return Result.Success();
        }

        public Result DelegateStake(
            List<BlockchainEvent> events,
            DelegateStakeData actionData,
            IUnitOfWork uow,
            Address senderAddress)
        {
            var firstEvent = events.First();

            var addressRepo = NewRepository<Address>(uow);

            firstEvent.Amount = actionData.Amount * -1;
            senderAddress.StakedBalance += actionData.Amount;
            senderAddress.AvailableBalance -= actionData.Amount;

            var validatorAddress = addressRepo.Get(a => a.BlockchainAddress == actionData.ValidatorAddress).SingleOrDefault();
            var newAddress = validatorAddress == null;

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

            events.Add(new BlockchainEvent
            {
                Address = validatorAddress,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Amount = actionData.Amount,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString()
            });

            if (newAddress)
                addressRepo.Insert(validatorAddress);
            else
                addressRepo.Update(validatorAddress);

            return Result.Success();
        }

        public Result ConfigureValidator(
            List<BlockchainEvent> events, 
            ConfigureValidatorData actionData, 
            IUnitOfWork uow,
            Address senderAddress)
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
                    IsActive = actionData.IsEnabled,
                    IsDeleted = false
                });
            }
            else
            {
                validator.NetworkAddress = actionData.NetworkAddress;
                validator.SharedRewardPercent = actionData.SharedRewardPercent;
                validator.IsActive = actionData.IsEnabled;
                validator.IsDeleted = false;
                validatorRepo.Update(validator);
            }

            var depositAmount = 10000 - senderAddress.DepositBalance;
            senderAddress.DepositBalance += depositAmount;
            senderAddress.AvailableBalance -= depositAmount;

            return Result.Success();   
        }

        public Result RemoveValidator(List<BlockchainEvent> events, IUnitOfWork uow, Address senderAddress)
        {
            var validatorRepo = NewRepository<Validator>(uow);
            var eventRepo = NewRepository<BlockchainEvent>(uow);
            var addressRepo = NewRepository<Address>(uow);

            var validator = validatorRepo
                .Get(v => v.BlockchainAddress == senderAddress.BlockchainAddress)
                .SingleOrDefault();
            if (validator == null)
                return Result.Failure("Address {0} is not a validator.".F(senderAddress.BlockchainAddress));

            validator.IsDeleted = true;
            validatorRepo.Update(validator);

            senderAddress.AvailableBalance += senderAddress.DepositBalance;
            senderAddress.DepositBalance = 0;

            var firstEvent = events.First();
            var delegateStakeIds = eventRepo.GetAs(
                e => e.EventType == EventType.Action.ToString()
                && e.AddressId == senderAddress.AddressId
                && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                && e.Fee == null,
                e => e.TxActionId);

            var delegateStakeEvents = eventRepo.Get(
                e => delegateStakeIds.Contains(e.TxActionId) && e.Fee != null,
                e => e.Address).GroupBy(e => e.Address);

            foreach(var group in delegateStakeEvents)
            {
                var address = group.Key;
                var stakedAmount = group.Sum(g => g.Amount ?? 0) * -1;

                events.Add(new BlockchainEvent {
                    AddressId = address.AddressId,
                    Amount = stakedAmount,
                    BlockId = firstEvent.BlockId,
                    TransactionId = firstEvent.TransactionId,
                    TxActionId = firstEvent.TxActionId,
                    EventType = EventType.StakeReturned.ToString()
                });

                address.StakedBalance -= stakedAmount;
                address.AvailableBalance += stakedAmount;

                addressRepo.Update(address);
            }

            return Result.Success();
        }

        public Result SetAssetCode(List<BlockchainEvent> events, SetAssetCodeData actionData, IUnitOfWork uow)
        {
            var assetRepo = NewRepository<Asset>(uow);

            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));
            asset.AssetCode = actionData.AssetCode;
            assetRepo.Update(asset);

            events.First().AssetId = asset.AssetId;

            return Result.Success();          
        }

        public Result SetAssetController(
            List<BlockchainEvent> events,
            SetAssetControllerData actionData,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var assetRepo = NewRepository<Asset>(uow);

            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure<IEnumerable<BlockchainEvent>>(
                    "Asset {0} does not exist.".F(actionData.AssetHash));
            asset.ControllerAddress = actionData.ControllerAddress;

            var firstEvent = events.First();
            firstEvent.AssetId = asset.AssetId;

            var controllerAddress = addressRepo
                .Get(a => a.BlockchainAddress == actionData.ControllerAddress)
                .SingleOrDefault();

            if (controllerAddress == null)
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

            events.Add(new BlockchainEvent
            {
                Address = controllerAddress,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Fee = 0,
                Amount = 0,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString(),
                AssetId = asset.AssetId
            });

            assetRepo.Update(asset);

            return Result.Success();
        }

        public Result SetAccountController(
            List<BlockchainEvent> events,
            SetAccountControllerData actionData,
            IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var accountRepo = NewRepository<Account>(uow);

            var account = accountRepo.Get(a => a.Hash == actionData.AccountHash).SingleOrDefault();
            if (account == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.AccountHash));
            account.ControllerAddress = actionData.ControllerAddress;

            var firstEvent = events.First();
            firstEvent.AccountId = account.AccountId;

            var controllerAddress = addressRepo
                .Get(a => a.BlockchainAddress == actionData.ControllerAddress)
                .SingleOrDefault();
            if (controllerAddress == null)
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

            events.Add(new BlockchainEvent
            {
                Address = controllerAddress,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Fee = 0,
                Amount = 0,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString(),
                AccountId = account.AccountId
            });

            accountRepo.Update(account);

            return Result.Success();
        }

        public Result TransferAsset(List<BlockchainEvent> events, TransferAssetData actionData, IUnitOfWork uow)
        {
            var addressRepo = NewRepository<Address>(uow);
            var accountRepo = NewRepository<Account>(uow);
            var assetRepo = NewRepository<Asset>(uow);
            var holdingRepo = NewRepository<HoldingEligibility>(uow);

            var fromAccount = accountRepo.Get(a => a.Hash == actionData.FromAccountHash).SingleOrDefault();
            if (fromAccount == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.FromAccountHash));

            var toAccount = accountRepo.Get(a => a.Hash == actionData.ToAccountHash).SingleOrDefault();
            if (toAccount == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.ToAccountHash));

            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var fromHolding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == fromAccount.AccountId)
                .SingleOrDefault();
            if (fromHolding == null || fromHolding.Balance < actionData.Amount)
                return Result.Failure("Account {0} does not sufficient holding.".F(actionData.FromAccountHash));

            var toHolding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == toAccount.AccountId)
                .SingleOrDefault();
            var isNewHolding = toHolding == null;

            if (isNewHolding)
            {
                toHolding = new HoldingEligibility()
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

            var firstEvent = events.First();
            firstEvent.AccountId = fromAccount.AccountId;
            firstEvent.AssetId = asset.AssetId;
            firstEvent.Amount = actionData.Amount * -1;

            var toControllerAddressId = addressRepo
                .GetAs(a => a.BlockchainAddress == toAccount.ControllerAddress, a => a.AddressId)
                .SingleOrDefault();

            if (toControllerAddressId == default(long))
                return Result.Failure("Address {0} does not exist.".F(toAccount.ControllerAddress));

            events.Add(new BlockchainEvent
            {
                AddressId = toControllerAddressId,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Fee = 0,
                Amount = actionData.Amount,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString(),
                AccountId = toAccount.AccountId,
                AssetId = asset.AssetId
            });

            holdingRepo.Update(fromHolding);
            if (isNewHolding)
                holdingRepo.Insert(toHolding);
            else
                holdingRepo.Update(toHolding);
           
            return Result.Success();
        }

        public Result CreateAssetEmission(
            List<BlockchainEvent> events,
            CreateAssetEmissionData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.EmissionAccountHash)
                .SingleOrDefault();
            if (account == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.EmissionAccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var holding = new HoldingEligibility()
            {
                AssetHash = asset.Hash,
                AssetId = asset.AssetId,
                AccountHash = account.Hash,
                AccountId = account.AccountId,
                Balance = actionData.Amount
            };

            var firstEvent = events.First();
            firstEvent.AccountId = account.AccountId;
            firstEvent.AssetId = asset.AssetId;
            firstEvent.Amount = actionData.Amount;

            NewRepository<HoldingEligibility>(uow).Insert(holding); 

            return Result.Success();  
        }

        public Result CreateAsset(
            List<BlockchainEvent> events, 
            IUnitOfWork uow, 
            Address senderAddress, 
            TxAction action)
        {
            var addressRepo = NewRepository<Address>(uow);

            var asset = new Asset()
            {
                Hash = _blockchainCryptoProvider.DeriveHash(
                    senderAddress.BlockchainAddress, 
                    senderAddress.Nonce, 
                    (short)action.ActionNumber),
                ControllerAddress = senderAddress.BlockchainAddress
            };

            NewRepository<Asset>(uow).Insert(asset);
            events.First().Asset = asset;

            return Result.Success();
        }

        public Result CreateAccount(
            List<BlockchainEvent> events, 
            IUnitOfWork uow, 
            Address senderAddress, 
            TxAction action)
        {
            var addressRepo = NewRepository<Address>(uow);

            var account = new Account()
            {
                Hash = _blockchainCryptoProvider.DeriveHash(
                    senderAddress.BlockchainAddress,
                    senderAddress.Nonce,
                    (short)action.ActionNumber),
                ControllerAddress = senderAddress.BlockchainAddress
            };

            NewRepository<Account>(uow).Insert(account);
            events.First().Account = account;

            return Result.Success();
        }

        public Result SubmitVote(
            List<BlockchainEvent> events,
            SubmitVoteData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var firstEvent = events.First();
            firstEvent.AccountId = account.AccountId;
            firstEvent.AssetId = asset.AssetId;

            return Result.Success();
        }

        public Result SubmitVoteWeight(
            List<BlockchainEvent> events,
            SubmitVoteWeightData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var firstEvent = events.First();
            firstEvent.AccountId = account.AccountId;
            firstEvent.AssetId = asset.AssetId;

            return Result.Success();
        }

        public Result SetAccountEligibility(
            List<BlockchainEvent> events,
            SetAccountEligibilityData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var holdingRepo = NewRepository<HoldingEligibility>(uow);
            var holding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == account.AccountId)
                .SingleOrDefault();
                var isNewHolding = holding == null;

            if (isNewHolding)
            {
                holding = new HoldingEligibility()
                {
                    AssetHash = asset.Hash,
                    AssetId = asset.AssetId,
                    AccountHash = account.Hash,
                    AccountId = account.AccountId
                };
            }

            holding.IsPrimaryEligible = actionData.IsPrimaryEligible;
            holding.IsSecondaryEligible = actionData.IsSecondaryEligible;

            var firstEvent = events.First();
            firstEvent.AccountId = account.AccountId;
            firstEvent.AssetId = asset.AssetId;

            if (isNewHolding)
                holdingRepo.Insert(holding);
            else
                holdingRepo.Update(holding);

            return Result.Success();
        }

        public Result SetAssetEligibility(
            List<BlockchainEvent> events,
            SetAssetEligibilityData actionData,
            IUnitOfWork uow)
        {
            var assetRepo = NewRepository<Asset>(uow);
            var asset = assetRepo.Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            asset.IsEligibilityRequired = actionData.IsEligibilityRequired;
            assetRepo.Update(asset);

            var firstEvent = events.First();
            firstEvent.AssetId = asset.AssetId;

            return Result.Success();
        }

        public Result ChangeKycControllerAddress(
            List<BlockchainEvent> events,
            ChangeKycControllerAddressData actionData,
            IUnitOfWork uow)
        {
            var account = NewRepository<Account>(uow)
                .Get(a => a.Hash == actionData.AccountHash)
                .SingleOrDefault();
            if (account == null)
                return Result.Failure("Account {0} does not exist.".F(actionData.AccountHash));

            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var holdingRepo = NewRepository<HoldingEligibility>(uow);
            var holding = holdingRepo
                .Get(h => h.AssetId == asset.AssetId && h.AccountId == account.AccountId)
                .SingleOrDefault();
            var isNewHolding = holding == null;

            if (isNewHolding)
            {
                holding = new HoldingEligibility()
                {
                    AssetHash = asset.Hash,
                    AssetId = asset.AssetId,
                    AccountHash = account.Hash,
                    AccountId = account.AccountId
                };
            }

            holding.KycControllerAddress = actionData.KycControllerAddress;

            var firstEvent = events.First();
            firstEvent.AccountId = account.AccountId;
            firstEvent.AssetId = asset.AssetId;

            var kycControllerAddress = NewRepository<Address>(uow)
                .Get(a => a.BlockchainAddress == actionData.KycControllerAddress)
                .SingleOrDefault();

            if (kycControllerAddress == null)
                return Result.Failure("Address {0} does not exist.".F(actionData.KycControllerAddress));

            events.Add(new BlockchainEvent()
            {
                AddressId = kycControllerAddress.AddressId,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Fee = 0,
                Amount = 0,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString(),
                AccountId = account.AccountId,
                AssetId = asset.AssetId
            });

            if (isNewHolding)
                holdingRepo.Insert(holding);
            else
                holdingRepo.Update(holding);

            return Result.Success();
        }

        public Result AddKycProvider(
            List<BlockchainEvent> events,
            AddKycProviderData actionData,
            IUnitOfWork uow)
        {
            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var firstEvent = events.First();
            firstEvent.AssetId = asset.AssetId;

            var providerAddress = NewRepository<Address>(uow)
                .Get(a => a.BlockchainAddress == actionData.ProviderAddress)
                .SingleOrDefault();

            if (providerAddress == null)
                return Result.Failure("Address {0} does not exist.".F(actionData.ProviderAddress));

            events.Add(new BlockchainEvent()
            {
                AddressId = providerAddress.AddressId,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Fee = 0,
                Amount = 0,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString(),
                AssetId = asset.AssetId
            });

            return Result.Success();
        }

        public Result RemoveKycProvider(
            List<BlockchainEvent> events,
            RemoveKycProviderData actionData,
            IUnitOfWork uow)
        {
            var asset = NewRepository<Asset>(uow).Get(a => a.Hash == actionData.AssetHash).SingleOrDefault();
            if (asset == null)
                return Result.Failure("Asset {0} does not exist.".F(actionData.AssetHash));

            var firstEvent = events.First();
            firstEvent.AssetId = asset.AssetId;

            var providerAddress = NewRepository<Address>(uow)
                .Get(a => a.BlockchainAddress == actionData.ProviderAddress)
                .SingleOrDefault();

            if (providerAddress == null)
                return Result.Failure("Address {0} does not exist.".F(actionData.ProviderAddress));

            events.Add(new BlockchainEvent()
            {
                AddressId = providerAddress.AddressId,
                TxActionId = firstEvent.TxActionId,
                BlockId = firstEvent.BlockId,
                Fee = 0,
                Amount = 0,
                TransactionId = firstEvent.TransactionId,
                EventType = EventType.Action.ToString(),
                AssetId = asset.AssetId
            });

            return Result.Success();
        }
    }
}
