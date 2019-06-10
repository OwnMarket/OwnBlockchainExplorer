using Own.BlockchainExplorer.Common.Extensions;
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
    public class BlockInfoService : DataService, IBlockInfoService
    {

        public BlockInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {

        }

        public Result<IEnumerable<EquivocationInfoShortDto>> GetEquivocationsInfo(long blockNumber, 
            int page, 
            int limit)
        {
            using (var uow = NewUnitOfWork())
            {
               return Result.Success(
                   NewRepository<BlockchainEvent>(uow)
                    .Get(
                        e => e.Block.BlockNumber == blockNumber && e.EventType == EventType.DepositTaken.ToString(),
                        e => e.Equivocation,
                        e => e.Address)
                    .Select(e => new EquivocationInfoShortDto
                    {
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                        TakenDeposit = new DepositDto
                        {
                            BlockchainAddress = e.Address.BlockchainAddress,
                            Amount = e.Amount.Value * -1,
                            EquivocationProofHash = e.Equivocation.EquivocationProofHash
                        }
                    })
                    .Skip(page-1).Take(limit)
                );
            }
        }

        public Result<IEnumerable<TxInfoShortDto>> GetTransactionsInfo(long blockNumber, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    NewRepository<BlockchainEvent>(uow)
                     .Get(
                         e => e.Block.BlockNumber == blockNumber && e.EventType == EventType.Action.ToString(),
                         e => e.Transaction,
                         e => e.Address)
                    .GroupBy(e => e.Transaction)
                    .Skip(page - 1).Take(limit)
                    .Select(g => new TxInfoShortDto
                    {
                        Hash = g.Key.Hash,
                        NumberOfActions = g.Select(e => e.TxActionId).Distinct().Count(),
                        SenderAddress = g.First().Address.BlockchainAddress,
                        BlockNumber = blockNumber,
                        Status = g.Key.Status
                    })                   
                 );
            }
        }

        public Result<IEnumerable<StakingRewardDto>> GetStakingRewardInfo(long blockNumber, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(
                    NewRepository<BlockchainEvent>(uow)
                     .Get(
                         e => e.Block.BlockNumber == blockNumber && e.EventType == EventType.StakingReward.ToString(),
                         e => e.Address)
                    .Skip(page - 1).Take(limit)
                    .Select(e => new StakingRewardDto
                    {
                        StakerAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value
                    })
                 );
            }
        }

        public Result<BlockInfoDto> GetBlockInfo(long blockNumber)
        {
            using (var uow = NewUnitOfWork())
            {
                var block = NewRepository<Block>(uow)
                    .Get(b => b.BlockNumber == blockNumber, b => b.Validator, b => b.PreviousBlock)
                    .SingleOrDefault();

                if (block is null)
                    return Result.Failure<BlockInfoDto>("Block {0} does not exist.".F(blockNumber));
 
                return Result.Success(BlockInfoDto.FromDomainModel(block));
            }
        }
    }
}
