using System.Collections.Generic;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockInfoRepository
    {
        IEnumerable<EquivocationInfoShortDto> GetEquivocationsInfo(long blockNumber, int page, int limit);
        IEnumerable<TxInfoShortDto> GetTransactionsInfo(long blockNumber, int page, int limit);
    }
}
