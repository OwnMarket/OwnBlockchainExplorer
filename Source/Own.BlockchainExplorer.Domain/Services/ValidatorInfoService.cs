﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                    return Result.Success(Enumerable.Empty<StakeDto>());

                return Result.Success(eventRepo
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
                );
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
                       && e.Transaction.Status == TxStatus.Success.ToString());

                var receivedStakes = eventRepo
                   .Get(
                       e => e.EventType == EventType.Action.ToString()
                       && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                       && e.Fee == null
                       && e.Transaction.Status == TxStatus.Success.ToString(),
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

                return Result.Success(validatorDtos.OrderByDescending(v => v.TotalStake).Skip((page - 1) * limit).Take(limit));
            }
        }
    }
}
