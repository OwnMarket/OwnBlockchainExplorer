using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAddressInfoService
    {
        Result<IEnumerable<ControlledAccountDto>> GetAccountsInfo(string blockchainAddress, int page, int limit);
        Result<IEnumerable<ControlledAssetDto>> GetAssetsInfo(string blockchainAddress, int page, int limit);
        Result<IEnumerable<StakeDto>> GetDelegatedStakesInfo(string blockchainAddress, int page, int limit);
        Result<IEnumerable<StakeDto>> GetReceivedStakesInfo(string blockchainAddress, int page, int limit);
        Result<IEnumerable<EventDto>> GetEventsInfo(string blockchainAddress, int page, int limit);
        Result<AddressInfoDto> GetAddressInfo(string blockchainAddress);
    }
}
