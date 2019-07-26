using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class AddressInfoRepositoryFactory : IAddressInfoRepositoryFactory
    {
        public IAddressInfoRepository Create(IUnitOfWork unitOfWork)
        {
            return new AddressInfoRepository((unitOfWork as UnitOfWork).Db);
        }
    }
}
