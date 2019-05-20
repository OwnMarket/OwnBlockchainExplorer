using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class BlockchainInfoRepositoryFactory : IBlockchainInfoRepositoryFactory
    {
        public IBlockchainInfoRepository Create(IUnitOfWork unitOfWork)
        {
            return new BlockchainInfoRepository((unitOfWork as UnitOfWork).Db);
        }
    }
}
