using System.Collections.Generic;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAssetBridgeService
    {
        Task<Result<List<BridgeTransferStatsInfoDto>>> GetBridgeTransferStats(string assetHash);
        Task<Result<List<BridgeTransferInfoDto>>> GetBridgeTransfers(string assetHash, int page, int limit);
    }
}