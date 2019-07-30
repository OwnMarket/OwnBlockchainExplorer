using System.Collections.Generic;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAddressInfoRepository
    {
        IEnumerable<ControlledAccountDto> GetAccountsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive);
        IEnumerable<ControlledAssetDto> GetAssetsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive);
        IEnumerable<StakeDto> GetDelegatedStakesInfo(string blockchainAddress, int page, int limit);
        IEnumerable<StakeDto> GetReceivedStakesInfo(string blockchainAddress, int page, int limit);
        EventsSummaryDto GetEventsInfo(string blockchainAddress, int page, int limit);
    }
}
