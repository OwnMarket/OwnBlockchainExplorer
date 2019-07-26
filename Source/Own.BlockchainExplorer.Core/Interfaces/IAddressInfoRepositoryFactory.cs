namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAddressInfoRepositoryFactory
    {
        IAddressInfoRepository Create(IUnitOfWork unitOfWork);
    }
}
