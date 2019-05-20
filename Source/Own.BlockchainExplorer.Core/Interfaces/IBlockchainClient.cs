using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using System.Threading.Tasks;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainClient
    {
        Task<Result<AddressDto>> GetAddressInfo(string blockchainAddress);
        Task<Result<BlockDto>> GetBlockInfo(long blockNumber);
        Task<Result<TxDto>> GetTxInfo(string txHash);
        Task<Result<EquivocationDto>> GetEquivocationInfo(string equivocationProofHash);
    }
}
