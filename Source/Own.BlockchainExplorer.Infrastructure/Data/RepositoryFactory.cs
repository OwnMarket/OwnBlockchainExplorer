using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepository<T> Create<T>(IUnitOfWork unitOfWork)
            where T : class
        {
            return new Repository<T>((unitOfWork as UnitOfWork).Db);
        }
    }
}
