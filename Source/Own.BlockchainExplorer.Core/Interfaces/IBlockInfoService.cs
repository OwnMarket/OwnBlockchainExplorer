﻿using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockInfoService
    {
        Result<BlockInfoDto> GetBlockInfo(long blockNumber);
        Result<BlockInfoDto> GetLastConfigBlock();
        Result<IEnumerable<EquivocationInfoShortDto>> GetEquivocationsInfo(long blockNumber, int page, int limit);
        Result<IEnumerable<TxInfoShortDto>> GetTransactionsInfo(long blockNumber, int page, int limit);
        Result<IEnumerable<StakingRewardDto>> GetStakingRewardInfo(long blockNumber, int page, int limit);
    }
}
