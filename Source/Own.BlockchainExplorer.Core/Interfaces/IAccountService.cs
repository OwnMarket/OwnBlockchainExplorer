using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAccountService
    {
        Result<List<AccountShortInfoDto>> GetAccounts(int page, int limit);
        Result<AccountShortInfoDto> GetAccountInfo(string accountHash);
        Result<List<AccountTransfersInfoDto>> GetAccountTransfers(string accountHash, int page, int limit);
        Result<List<AccountHoldingInfoDto>> GetAccountHoldings(string accountHash, int page, int limit);
    }
}