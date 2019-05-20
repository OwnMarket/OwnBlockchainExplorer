namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoRepositoryFactory
    {
        IBlockchainInfoRepository Create(IUnitOfWork unitOfWork);
    }
}
