using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ValidatorInfoService : DataService, IValidatorInfoService
    {
        public ValidatorInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<ValidatorInfoDto> GetValidatorInfo(string blockchainAddress)
        {
            using (var uow = NewUnitOfWork())
            {
                var validator = NewRepository<Validator>(uow)
                    .Get(v => v.BlockchainAddress == blockchainAddress && !v.IsDeleted)
                    .SingleOrDefault();

                if (validator is null)
                    return Result.Failure<ValidatorInfoDto>("Validator {0} does not exist.".F(blockchainAddress));

                var validatorDto = ValidatorInfoDto.FromDomainModel(validator);

                return Result.Success(validatorDto);
            }
        }

        public Result<IEnumerable<StakeDto>> GetStakesInfo(string blockchainAddress, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var receivedStakeIds = eventRepo
                    .GetAs(
                        e => e.EventType == EventType.Action.ToString()
                        && e.Address.BlockchainAddress == blockchainAddress
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Fee == null
                        && e.Tx.Status == TxStatus.Success.ToString(),
                        e => e.TxActionId);

                if (!receivedStakeIds.Any())
                    return Result.Success(Enumerable.Empty<StakeDto>());

                return eventRepo
                    .Get(e => receivedStakeIds.Contains(e.TxActionId) && e.Fee != null, e => e.Address)
                    .GroupBy(e => e.Address)
                    .Select(g => new StakeDto
                    {
                        StakerAddress = g.Key.BlockchainAddress,
                        Amount = g.Sum(e => e.Amount.Value) * -1,
                        ValidatorAddress = blockchainAddress
                    })
                    .OrderByDescending(s => s.Amount)
                    .Skip((page - 1) * limit).Take(limit)
                    .To(Result.Success);
            }
        }

        public Result<IEnumerable<ValidatorInfoShortDto>> GetValidators(int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var validatorDtos = new List<ValidatorInfoShortDto>();

                var eventRepo = NewRepository<BlockchainEvent>(uow);
                var delegatedStakes = eventRepo
                   .Get(
                       e => e.EventType == EventType.Action.ToString()
                       && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                       && e.Fee != null
                       && e.Tx.Status == TxStatus.Success.ToString());

                var receivedStakes = eventRepo
                   .Get(
                       e => e.EventType == EventType.Action.ToString()
                       && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                       && e.Fee == null
                       && e.Tx.Status == TxStatus.Success.ToString(),
                       e => e.Address);

                var validators = NewRepository<Validator>(uow).Get(v => !v.IsDeleted);

                foreach (var validator in validators)
                {
                    var localStakeEvents = receivedStakes
                        .Where(e => e.Address.BlockchainAddress == validator.BlockchainAddress);
                    var localStakeActionIds = localStakeEvents.Select(e => e.TxActionId);
                    var numberOfStakers = delegatedStakes
                        .Where(e => localStakeActionIds.Contains(e.TxActionId))
                        .GroupBy(e => e.AddressId)
                        .Where(g => g.Sum(e => e.Amount.Value) != 0)
                        .Select(g => g.Key)
                        .Distinct()
                        .Count();

                    validatorDtos.Add(new ValidatorInfoShortDto
                    {
                        BlockchainAddress = validator.BlockchainAddress,
                        IsActive = validator.IsActive,
                        TotalStake = localStakeEvents.Sum(v => v.Amount.Value),
                        NumberOfStakers = numberOfStakers
                    });
                }

                return validatorDtos
                    .OrderByDescending(v => v.TotalStake)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .To(Result.Success);
            }
        }

        public Result<IEnumerable<ValidatorGeoInfoDto>> GetValidatorsGeo()
        {
            using (var uow = NewUnitOfWork())
            {
                var validatorsGeo = NewRepository<Validator>(uow)
                    .GetAs(v => !v.IsDeleted && v.GeoLocation != null, v => v.GeoLocation);

                if (validatorsGeo.IsNullOrEmpty())
                    return Result.Failure<IEnumerable<ValidatorGeoInfoDto>>("No geo location data found!");

                var validatorsGeoInfo = new List<ValidatorGeoInfoDto>();
                var alerts = new List<Alert>();
                foreach (var validatorGeo in validatorsGeo)
                {
                    try
                    {
                        validatorsGeoInfo.Add(JsonConvert.DeserializeObject<ValidatorGeoInfoDto>(validatorGeo));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        alerts.Add(Alert.Error(ex.LogFormat()));
                    }
                }

                return Result.Success(validatorsGeoInfo.AsEnumerable(), alerts);
            }
        }
    }
}
