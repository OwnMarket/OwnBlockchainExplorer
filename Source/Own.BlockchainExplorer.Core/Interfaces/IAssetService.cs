using System.Collections.Generic;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAssetService
    {
        Result<List<AssetShortInfoDto>> GetAssets(string accountHash, int page, int limit);
        Task<Result<AssetShortInfoDto>> GetAssetInfo(string assetHash);
        Result<List<AssetTransferInfoDto>> GetAssetTransfers(string assetHash, int page, int limit);
        Result<List<AssetHolderInfoDto>> GetAssetHolders(string assetHash, int page, int limit);
    }
}