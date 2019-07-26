using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class BlockInfoRepositoryFactory : IBlockInfoRepositoryFactory
    {
        public IBlockInfoRepository Create(IUnitOfWork unitOfWork)
        {
            return new BlockInfoRepository((unitOfWork as UnitOfWork).Db);
        }
    }
}
