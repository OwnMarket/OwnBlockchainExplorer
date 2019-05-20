using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoRepository
    {
        IEnumerable<TxInfoShortDto> GetTxs(int limit, int page);
        IEnumerable<BlockInfoShortDto> GetBlocks(int limit, int page);
    }
}
