using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockInfoService
    {
        Result<BlockInfoDto> GetBlockInfo(long blockNumber);
        Result<IEnumerable<EquivocationInfoShortDto>> GetEquivocationsInfo(long blockNumber);
        Result<IEnumerable<TxInfoShortDto>> GetTransactionsInfo(long blockNumber);
        Result<IEnumerable<StakingRewardDto>> GetStakingRewardInfo(long blockNumber);
    }
}
