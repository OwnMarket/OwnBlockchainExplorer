﻿using Own.BlockchainExplorer.Common.Extensions;
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
    public class TxInfoService : DataService, ITxInfoService
    {
        public TxInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {

        }

        public Result<TxInfoDto> GetTxInfo(string txHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Transaction.Hash == txHash,
                    e => e.Transaction,
                    e => e.Block,
                    e => e.Address);
                if (!events.Any())
                    return Result.Failure<TxInfoDto>("Transaction {0} does not exist.".F(txHash));

                var txDto = TxInfoDto.FromDomainModel(events.FirstOrDefault().Transaction);
                txDto.BlockNumber = events.FirstOrDefault().Block.BlockNumber;
                txDto.SenderAddress = events.FirstOrDefault().Address.BlockchainAddress;
                txDto.NumberOfActions = events
                    .Where(e => e.EventType == EventType.Action.ToString())
                    .GroupBy(e => e.TxActionId)
                    .Count();

                return Result.Success(txDto);
            }
        }

        public Result<IEnumerable<ActionDto>> GetActionsInfo(string txHash, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    NewRepository<BlockchainEvent>(uow)
                        .Get(
                            e => e.Transaction.Hash == txHash && e.EventType == EventType.Action.ToString(),
                            e => e.Transaction,
                            e => e.TxAction)
                        .GroupBy(e => e.TxActionId)
                        .Skip((page - 1) * limit).Take(limit)
                        .Select(g => ActionDto.FromDomainModel(g.First().TxAction))     
                );
            }
        }
    }
}
