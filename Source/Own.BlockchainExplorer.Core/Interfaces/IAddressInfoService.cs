using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAddressInfoService
    {
        Result<IEnumerable<ControlledAccountDto>> GetAccountsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive);
        Result<IEnumerable<ControlledAssetDto>> GetAssetsInfo(
            string blockchainAddress,
            int page,
            int limit,
            bool? isActive);
        Result<StakeSummaryDto> GetDelegatedStakesInfo(string blockchainAddress, int page, int limit);
        Result<StakeSummaryDto> GetReceivedStakesInfo(string blockchainAddress, int page, int limit);
        Result<EventsSummaryDto> GetEventsInfo(string blockchainAddress, string filter, int page, int limit);
        Result<FileDto> GetEventsCSV(string blockchainAddress);
        Result<AddressInfoDto> GetAddressInfo(string blockchainAddress);
        Result<decimal> GetTotalChxBalanceInfo(string blockchainAddress);
        Result<decimal> GetAvailableChxBalanceInfo(string blockchainAddress);
    }
}
