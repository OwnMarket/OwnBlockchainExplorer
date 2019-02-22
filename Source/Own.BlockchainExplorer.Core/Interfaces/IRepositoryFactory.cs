namespace Own.BlockchainExplorer.Core.Interfaces
{
    // For generic entity repository
    public interface IRepositoryFactory
    {
        IRepository<TEntity> Create<TEntity>(IUnitOfWork unitOfWork)
            where TEntity : class;
    }

    // For custom repositories
    public interface IRepositoryFactory<out TRepository>
        where TRepository : class
    {
        TRepository Create(IUnitOfWork unitOfWork);
    }

    #region Custom repositories

    /* Example
    public interface IMyCustomRepositoryFactory : IRepositoryFactory<IMyCustomRepository>
    {
    }
    */

    #endregion
}
