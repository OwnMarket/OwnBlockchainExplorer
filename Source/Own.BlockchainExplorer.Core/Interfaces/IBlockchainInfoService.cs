using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoService
    {
        Result<AddressInfoDto> GetAddressInfo(string blockchainAddress);
    }
}
