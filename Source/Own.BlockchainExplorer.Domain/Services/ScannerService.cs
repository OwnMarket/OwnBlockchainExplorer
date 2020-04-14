using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Common;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ScannerService : DataService, IScannerService
    {
        private readonly IBlockchainClient _blockchainClient;
        private readonly IImportService _importService;
        private readonly IGeoLocationService _geoLocationService;

        public ScannerService(
            IBlockchainClient blockchainClient,
            IImportService importService,
            IGeoLocationService geoLocationService,
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainClient = blockchainClient;
            _importService = importService;
            _geoLocationService = geoLocationService;
        }

        public Result InitialBlockchainConfiguration()
        {
            try
            {
                using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
                {
                    var addressRepo = NewRepository<Address>(uow);
                    var validatorRepo = NewRepository<Validator>(uow);

                    if (addressRepo.Exists(a => true))
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

                    foreach (var validatorString in Config.GenesisValidators)
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
                            NetworkAddress = validatorValues[1],
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
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return Result.Success();
        }

        public async Task<Result> CheckNewBlocks()
        {
            try
            {
                long lastBlockNumber;
                using (var uow = NewUnitOfWork())
                {
                    var lastBlockNumbers = NewRepository<Block>(uow)
                        .GetLastAs(b => true, b => b.BlockNumber, 1);

                    lastBlockNumber = lastBlockNumbers.Any()
                        ? lastBlockNumbers.SingleOrDefault()
                        : -1;
                }

                var getBlocksTasks = new List<Task<BlockDto>>();
                for (var blockNr = lastBlockNumber + 1; blockNr <= lastBlockNumber + Config.ScanBatchSize; blockNr ++)
                {
                    getBlocksTasks.Add(GetBlock(blockNr));
                }

                Task.WaitAll(getBlocksTasks.ToArray());

                var newBlocks = getBlocksTasks
                    .Where(t => t.IsCompletedSuccessfully)
                    .Select(t => t.Result)
                    .Where(b => !(b is null))
                    .OrderBy(b => b.Number);

                foreach(var newBlock in newBlocks)
                {
                    Log.Info($"Importing block {newBlock.Number} started.");

                    var blockResult = await ProcessBlock(newBlock);
                    if (blockResult.Failed)
                        return Result.Failure(blockResult.Alerts);

                    Log.Info($"Importing block {newBlock.Number} finished.");
                }

                return Result.Success();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return Result.Failure(e.LogFormat(true));
            }
        }

        private async Task<Result> ProcessBlock(BlockDto blockDto)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                uow.BeginTransaction();

                var validatorReward = 0M;

                if (blockDto.Configuration != null)
                {
                    var configResult = ProcessConfiguration(blockDto.Configuration, uow);
                    if (configResult.Failed)
                        return Result.Failure(configResult.Alerts);

                    var geoLocationResult = ProcessGeoLocation(uow);
                    if (geoLocationResult.Failed)
                        return Result.Failure(geoLocationResult.Alerts);
                }

                var blockImportResult = _importService.ImportBlock(blockDto, uow);
                if (blockImportResult.Failed)
                    return Result.Failure(blockImportResult.Alerts);

                foreach (var txHash in blockDto.TxSet)
                {
                    var txResult = await ProcessTransaction(txHash, blockImportResult.Data, uow);
                    if (txResult.Failed)
                        return Result.Failure(txResult.Alerts);

                    validatorReward += txResult.Data;
                }

                foreach (var equivocationProof in blockDto.EquivocationProofs)
                {
                    var equivocationResult = await ProcessEquivocation(equivocationProof,
                        blockImportResult.Data.BlockId,
                        uow);
                    if (equivocationResult.Failed)
                        return Result.Failure(equivocationResult.Alerts);
                }

                if (blockDto.Configuration != null && !blockDto.Configuration.DormantValidators.IsNullOrEmpty())
                {
                    foreach (var validatorAddress in blockDto.Configuration.DormantValidators)
                    {
                        var importResult = _importService
                            .ImportDormantValidatorEvents(validatorAddress, blockImportResult.Data.BlockId, uow);
                        if (importResult.Failed)
                            return Result.Failure(importResult.Alerts);
                    }
                }

                foreach (var stakingReward in blockDto.StakingRewards)
                {
                    var importResult = _importService
                        .ImportStakingRewardEvent(stakingReward, blockImportResult.Data.BlockId, uow);
                    if (importResult.Failed)
                        return Result.Failure(importResult.Alerts);
                }

                var totalStakingRewards = blockDto.StakingRewards.Sum(s => s.Amount);

                var validatorRewardResult = _importService.ImportValidatorRewardEvent(
                    validatorReward - totalStakingRewards,
                    blockImportResult.Data.BlockId,
                    blockDto.ProposerAddress,
                    uow);

                if (validatorRewardResult.Failed)
                    return Result.Failure(validatorRewardResult.Alerts);

                uow.CommitTransaction();

                return Result.Success();
            }
        }

        private Result ProcessGeoLocation(IUnitOfWork uow)
        {
            var validatorRepo = NewRepository<Validator>(uow);
            var validators = validatorRepo.Get(v => !v.IsDeleted);
            var alerts = new List<Alert>();
            foreach (var validator in validators)
            {
                var endCharPosition = validator.NetworkAddress.LastIndexOf(":") == -1
                    ? validator.NetworkAddress.Length
                    : validator.NetworkAddress.LastIndexOf(":");
                var validatorAddress = validator.NetworkAddress.Substring(0, endCharPosition);
                try
                {
                    var ipAddress =
                        Dns.GetHostAddresses(validatorAddress)
                        .OrderBy(a => a.AddressFamily)
                        .FirstOrDefault()?.ToString();

                    if (ipAddress.IsNullOrEmpty())
                        continue;

                    var geoLocationResult = _geoLocationService.GetGeoLocation(ipAddress).Result;
                    if (geoLocationResult.Successful)
                    {
                        var validatorGeoInfo =
                            new ValidatorGeoInfoDto
                            {
                                NetworkAddress = validatorAddress,
                                Location = geoLocationResult.Data
                            };

                        validator.GeoLocation = JsonConvert.SerializeObject(validatorGeoInfo);
                        validatorRepo.Update(validator);
                    }
                    else
                        alerts.AddRange(geoLocationResult.Alerts);
                }
                catch (Exception ex)
                {
                    Log.Error($"[{validatorAddress}]: {ex.LogFormat()}");
                    alerts.Add(Alert.Error($"{validatorAddress}:{ex.LogFormat(true)}"));
                }
            }
            uow.Commit();

            return Result.Success(alerts);
        }

        private Result ProcessConfiguration(ConfigurationDto configurationDto, IUnitOfWork uow)
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

            uow.Commit();

            return Result.Success();
        }

        private async Task<Result> ProcessEquivocation(string equivocationProof, long blockId, IUnitOfWork uow)
        {
            var equivocationResult = await _blockchainClient.GetEquivocationInfo(equivocationProof);
            if (equivocationResult.Failed)
                return Result.Failure(equivocationResult.Alerts);

            var importResult = _importService
                .ImportEquivocation(equivocationResult.Data, blockId, uow);
            if (importResult.Failed)
                return Result.Failure(importResult.Alerts);

            var depositTakenResult = _importService.ImportDepositTakenEvent(
                equivocationResult.Data,
                blockId,
                importResult.Data.EquivocationId,
                uow);
            if (depositTakenResult.Failed)
                return Result.Failure(depositTakenResult.Alerts);

            var depositGivenResult = _importService.ImportDepositGivenEvents(
                equivocationResult.Data,
                blockId,
                importResult.Data.EquivocationId,
                uow);
            if (depositGivenResult.Failed)
                return Result.Failure(depositGivenResult.Alerts);

            return Result.Success();
        }

        private async Task<Result<decimal>> ProcessTransaction(string txHash, Block block, IUnitOfWork uow)
        {
            var txResult = await _blockchainClient.GetTxInfo(txHash);
            if (txResult.Failed)
                return Result.Failure<decimal>(txResult.Alerts);
            var txDto = txResult.Data;

            var addressResult = _importService.ImportAddress(txDto.SenderAddress, txDto.Nonce, uow);
            if (addressResult.Failed)
                return Result.Failure<decimal>(addressResult.Alerts);
            var senderAddress = addressResult.Data;

            var txImportResult = _importService.ImportTx(txDto, block.Timestamp, uow);
            if (txImportResult.Failed)
                return Result.Failure<decimal>(txImportResult.Alerts);
            var transaction = txImportResult.Data;

            var i = 0;
            foreach (var actionDto in txDto.Actions)
            {
                var action = _importService.ImportAction(actionDto, ++i, uow).Data;
                var eventResult = _importService.ImportEvents(
                    action,
                    senderAddress,
                    block.BlockId,
                    transaction,
                    (JObject)actionDto.ActionData,
                    uow);

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
