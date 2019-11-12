using Own.BlockchainExplorer.Core.Dtos.Api;
using System;
using System.Collections.Generic;
using Own.BlockchainExplorer.Core.Models;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoRepository
    {
        IEnumerable<Tx> GetTxs(int limit, int page);
        IEnumerable<BlockInfoShortDto> GetBlocks(int limit, int page);
        List<KeyValuePair<DateTime, int>> GetTxPerDay(int numberOfDays);
        Dictionary<long, int> GetValidatorProposedBlockCount(long minTimestamp);
        Dictionary<long, int> GetValidatorProposedTxCount(long minTimestamp);
        Dictionary<string, decimal> GetReceivedStakes();
        Dictionary<string, decimal> GetValidatorRewards(long minTimestamp);
        Dictionary<long, decimal> GetValidatorStakingRewards(long minTimestamp);
    }
}
