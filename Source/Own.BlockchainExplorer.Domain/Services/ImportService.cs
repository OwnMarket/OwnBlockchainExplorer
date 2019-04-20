using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class ImportService : DataService, IImportService
    {
        public ImportService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result ImportAddress(AddressDto addressDto)
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var addressRepo = NewRepository<Address>(uow);

                if (addressRepo.Exists(a => a.BlockchainAddress == addressDto.BlockchainAddress))
                    return Result.Failure("Address {0} already exists.".F(addressDto.BlockchainAddress));

                var address =
                    new Address
                    {
                        BlockchainAddress = addressDto.BlockchainAddress,
                        Nonce = addressDto.Nonce
                    };

                addressRepo.Insert(address);
                uow.Commit();
            }

            return Result.Success();
        }
    }
}
