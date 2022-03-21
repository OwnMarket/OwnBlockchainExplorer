using System.Collections.Generic;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IAssetBridgeRepository
    {
        Task<List<BridgeTransferInfoDto>> GetBridgeTransfers(string assetHash, int page, int limit);
        Task<List<string>> GetContractAddresses(string assetHash, BlockchainCode blockchainCode);
    }
}