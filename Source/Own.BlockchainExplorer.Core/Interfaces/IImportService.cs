using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Scanning;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IImportService
    {
        Result ImportAddress(AddressDto addressDto);
    }
}
