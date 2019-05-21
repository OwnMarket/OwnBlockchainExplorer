using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAddressInfoService
    {
        Result<IEnumerable<ControlledAccountDto>> GetAccountsInfo(string blockchainAddress);
        Result<IEnumerable<ControlledAssetDto>> GetAssetsInfo(string blockchainAddress);
        Result<IEnumerable<StakeDto>> GetDelegatedStakesInfo(string blockchainAddress);
        Result<IEnumerable<StakeDto>> GetReceivedStakesInfo(string blockchainAddress);
        Result<EventsDto> GetEventsInfo(string blockchainAddress);
        Result<AddressInfoDto> GetAddressInfo(string blockchainAddress);
    }
}
