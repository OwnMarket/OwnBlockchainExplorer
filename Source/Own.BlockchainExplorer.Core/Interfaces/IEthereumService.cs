using System.Threading.Tasks;
using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IEthereumService
    {
        Task<decimal> GetCirculatingSupply(string contractAddress, BlockchainCode blockchainCode);
    }
}