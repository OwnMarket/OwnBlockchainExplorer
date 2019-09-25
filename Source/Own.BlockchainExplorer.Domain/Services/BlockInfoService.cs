using System.Linq;
using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class BlockInfoService : DataService, IBlockInfoService
    {
        private readonly IBlockInfoRepositoryFactory _blockInfoRepositoryFactory;

        public BlockInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IBlockInfoRepositoryFactory blockInfoRepositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockInfoRepositoryFactory = blockInfoRepositoryFactory;
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

        public Result<BlockInfoDto> GetLastConfigBlock()
        {
            using (var uow = NewUnitOfWork())
            {
                var blockRepo = NewRepository<Block>(uow);
                var block = blockRepo
                    .GetLastAs(b => true, b => b, 1)
                    .SingleOrDefault();

                if (block is null)
                    return Result.Failure<BlockInfoDto>("No block found");

                var configurationBlock = blockRepo
                    .Get(b => b.BlockNumber == block.ConfigurationBlockNumber, b => b.Validator, b => b.PreviousBlock)
                    .SingleOrDefault();

                if (configurationBlock is null)
                    return Result.Failure<BlockInfoDto>("ConfigurationBlock {0} does not exist.".F(configurationBlock.BlockNumber));

                return Result.Success(BlockInfoDto.FromDomainModel(configurationBlock));
            }
        }

        public Result<IEnumerable<EquivocationInfoShortDto>> GetEquivocationsInfo(
            long blockNumber,
            int page,
            int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var equivocationsInfo = _blockInfoRepositoryFactory.Create(uow)
                    .GetEquivocationsInfo(blockNumber, page, limit);
                return Result.Success(equivocationsInfo);
            }
        }

        public Result<IEnumerable<TxInfoShortDto>> GetTransactionsInfo(long blockNumber, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var transactionInfo = _blockInfoRepositoryFactory.Create(uow)
                    .GetTransactionsInfo(blockNumber, page, limit);
                return Result.Success(transactionInfo);
            }
        }

        public Result<IEnumerable<StakingRewardDto>> GetStakingRewardInfo(long blockNumber, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var stakingRewardInfo = NewRepository<BlockchainEvent>(uow)
                    .GetLastAs(
                         e => e.Block.BlockNumber == blockNumber && e.EventType == EventType.StakingReward.ToString(),
                         e => true,
                         e => new StakingRewardDto
                         {
                             StakerAddress = e.Address.BlockchainAddress,
                             Amount = e.Amount.Value
                         },
                         (page - 1) * limit,
                         limit);
                return Result.Success(stakingRewardInfo);
            }
        }
    }
}
