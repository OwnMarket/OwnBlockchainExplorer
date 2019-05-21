using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface ITxInfoService
    {
        Result<TxInfoDto> GetTxInfo(string txHash);
        Result<IEnumerable<ActionDto>> GetActionsInfo(string txHash);
    }
}
