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
        StakeSummaryDto GetDelegatedStakesInfo(string blockchainAddress, int page, int limit);
        StakeSummaryDto GetReceivedStakesInfo(string blockchainAddress, int page, int limit);
        EventsSummaryDto GetEventsInfo(string blockchainAddress, string filter, int page, int limit);
    }
}
