namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockInfoRepositoryFactory
    {
        IBlockInfoRepository Create(IUnitOfWork unitOfWork);
    }
}
