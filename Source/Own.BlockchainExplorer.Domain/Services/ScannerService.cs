using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ScannerService : DataService, IScannerService
    {
        private readonly IBlockchainClient _blockchainClient;
        private readonly IImportService _importService;

        public ScannerService(
            IBlockchainClient blockchainClient,
            IImportService importService,
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainClient = blockchainClient;
            _importService = importService;
        }

        public Result InitialBlockchainConfiguration()
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var addressRepo = NewRepository<Address>(uow);
                var validatorRepo = NewRepository<Validator>(uow);

                if(addressRepo.Exists(a => true)) 
                    return Result.Success();

                var addresses = new List<Address>();
                var validators = new List<Validator>();

                addresses.Add(new Address
                {
                    BlockchainAddress = Config.GenesisAddress,
                    AvailableBalance = Config.GenesisChxSupply.Value,
                    StakedBalance = 0,
                    DepositBalance = 0,
                    Nonce = 0
                });

                foreach(var validatorString in Config.GenesisValidators)
                {
                    var validatorValues = validatorString.Split("@");

                    addresses.Add(new Address
                    {
                        BlockchainAddress = validatorValues[0],
                        AvailableBalance = 0,
                        StakedBalance = 0,
                        DepositBalance = 0,
                        Nonce = 0
                    });

                    validators.Add(new Validator
                    {
                        BlockchainAddress = validatorValues[0],
                        NetworkAddress = "http://" + validatorValues[1],
                        IsActive = true,
                        SharedRewardPercent = 0
                    });
                }

                var fakeValidator = Config.FakeValidator.Split("@");

                addresses.Add(new Address
                {
                    BlockchainAddress = fakeValidator[0],
                    AvailableBalance = 0,
                    StakedBalance = 0,
                    DepositBalance = 0,
                    Nonce = 0
                });

                validators.Add(new Validator
                {
                    BlockchainAddress = fakeValidator[0],
                    NetworkAddress = fakeValidator[1],
                    IsActive = false,
                    IsDeleted = true,
                    SharedRewardPercent = 0
                });

                addressRepo.Insert(addresses);
                validatorRepo.Insert(validators);

                uow.Commit();

                return Result.Success();
            }
        }

        public async Task<Result> CheckNewBlocks()
        {
            long lastBlockNumber = 0;
            using (var uow = NewUnitOfWork())
            {
                lastBlockNumber = NewRepository<Block>(uow)
                    .GetLastAs(b => true, b => b.BlockNumber, 1)
                    .SingleOrDefault();
                if (lastBlockNumber == 0) lastBlockNumber = -1;
            }
            var newBlock = await GetBlock(lastBlockNumber + 1);

            while (newBlock != null)
            {
                Console.WriteLine($"Importing block {newBlock.Number} started.");
                var blockResult = await ProcessBlock(newBlock);
                if (blockResult.Failed)
                    return Result.Failure(blockResult.Alerts);

                lastBlockNumber = newBlock.Number;
                newBlock = await GetBlock(lastBlockNumber + 1);
                Console.WriteLine($"Importing block {newBlock.Number} finished.");
            }

            if (newBlock is null)
                return Result.Success();

            return Result.Success();
        }

        private async Task<Result> ProcessBlock(BlockDto blockDto)
        {
            var validatorReward = 0M;

            if (blockDto.Configuration != null)
            {
                var configResult = ProcessConfiguration(blockDto.Configuration);
                if (configResult.Failed)
                    return Result.Failure(configResult.Alerts);
            }

            var blockImportResult = _importService.ImportBlock(blockDto);
            if (blockImportResult.Failed)
                return Result.Failure(blockImportResult.Alerts);

            foreach (var txHash in blockDto.TxSet)
            {
                var txResult = await ProcessTransaction(txHash, blockImportResult.Data);
                if (txResult.Failed)
                    return Result.Failure(txResult.Alerts);

                validatorReward += txResult.Data;
            }

            foreach (var equivocationProof in blockDto.EquivocationProofs)
            {
                var equivocationResult = await ProcessEquivocation(equivocationProof, blockImportResult.Data.BlockId);
                if (equivocationResult.Failed)
                    return Result.Failure(equivocationResult.Alerts);
            }

            foreach (var stakingReward in blockDto.StakingRewards)
            {
                var importResult = _importService
                    .ImportStakingRewardEvent(stakingReward, blockImportResult.Data.BlockId);
                if (importResult.Failed)
                    return Result.Failure(importResult.Alerts);
            }

            var validatorRewardResult = _importService.ImportValidatorRewardEvent(
                validatorReward, 
                blockImportResult.Data.BlockId, 
                blockDto.ProposerAddress);

            if (validatorRewardResult.Failed)
                return Result.Failure(validatorRewardResult.Alerts);

            return Result.Success();
        }

        private Result ProcessConfiguration(ConfigurationDto configurationDto)
        {
            List<string> newAddressHashes = new List<string>();
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var validatorRepo = NewRepository<Validator>(uow);

                var validatorAddresses = configurationDto.Validators.Select(v => v.ValidatorAddress);
                var validators = validatorRepo.Get(v => validatorAddresses.Contains(v.BlockchainAddress) && !v.IsActive);
                var inactiveValidators = validatorRepo.Get(v => !validatorAddresses.Contains(v.BlockchainAddress) && v.IsActive);

                foreach (var validator in validators)
                {
                    validator.IsActive = true;
                    validatorRepo.Update(validator);
                }

                foreach (var validator in inactiveValidators)
                {
                    validator.IsActive = false;
                    validatorRepo.Update(validator);
                }
            }

            return Result.Success();
        }

        private async Task<Result> ProcessEquivocation(string equivocationProof, long blockId)
        {
            var equivocationResult = await _blockchainClient.GetEquivocationInfo(equivocationProof);
            if (equivocationResult.Failed)
                return Result.Failure(equivocationResult.Alerts);

            var importResult = _importService
                .ImportEquivocation(equivocationResult.Data, blockId);
            if (importResult.Failed)
                return Result.Failure(importResult.Alerts);

            var depositTakenResult = _importService.ImportDepositTakenEvent(
                equivocationResult.Data,
                blockId,
                importResult.Data.EquivocationId);
            if (depositTakenResult.Failed)
                return Result.Failure(depositTakenResult.Alerts);

            var depositGivenResult = _importService.ImportDepositGivenEvents(
                equivocationResult.Data,
                blockId,
                importResult.Data.EquivocationId);
            if (depositGivenResult.Failed)
                return Result.Failure(depositGivenResult.Alerts);

            return Result.Success();
        }

        private async Task<Result<decimal>> ProcessTransaction(string txHash, Block block)
        {
            var txResult = await _blockchainClient.GetTxInfo(txHash);
            if (txResult.Failed)
                return Result.Failure<decimal>(txResult.Alerts);
            var txDto = txResult.Data;

            var addressResult = _importService.ImportAddress(txResult.Data.SenderAddress, txResult.Data.Nonce);
            if (addressResult.Failed)
                return Result.Failure<decimal>(addressResult.Alerts);
            var senderAddress = addressResult.Data;

            var txImportResult = _importService.ImportTx(txResult.Data, block.Timestamp);
            if (txImportResult.Failed)
                return Result.Failure<decimal>(txImportResult.Alerts);
            var transaction = txImportResult.Data;

            var i = 0;
            foreach (var actionDto in txResult.Data.Actions)
            {
                var action = _importService.ImportAction(actionDto, ++i).Data;
                var eventResult = _importService.ImportEvents(
                    action, 
                    senderAddress, 
                    block, 
                    transaction, 
                    (JObject)actionDto.ActionData);

                if (eventResult.Failed)
                    return Result.Failure<decimal>(eventResult.Alerts);
            }

            return Result.Success(txDto.ActionFee * txDto.Actions.Count);
        }

        private async Task<BlockDto> GetBlock(long blockNumber)
        {
            var result = await _blockchainClient.GetBlockInfo(blockNumber);
            if (!result.Successful || result.Data.Hash is null)
                return null;

            return result.Data;
        }
    }
}
