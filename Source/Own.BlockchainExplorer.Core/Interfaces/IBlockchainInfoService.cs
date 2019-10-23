using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoService
    {
        Result<EquivocationInfoDto> GetEquivocationInfo(string equivocationProofHash);
        Result<AccountInfoDto> GetAccountInfo(string accountHash);
        Result<AssetInfoDto> GetAssetInfo(string assetHash);

        Result<IEnumerable<TxInfoShortDto>> GetTxs(int limit, int page);
        Result<IEnumerable<BlockInfoShortDto>> GetBlocks(int limit, int page);

        Result<string> Search(string searchValue);
    }
}
