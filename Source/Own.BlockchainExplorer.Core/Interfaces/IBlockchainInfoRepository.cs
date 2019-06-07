using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoRepository
    {
        IEnumerable<Transaction> GetTxs(int limit, int page);
        IEnumerable<BlockInfoShortDto> GetBlocks(int limit, int page);
    }
}
