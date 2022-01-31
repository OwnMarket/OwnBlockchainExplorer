using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class EthereumService : IEthereumService
    {
        private readonly IWeb3Provider _web3Provider;

        public EthereumService(IWeb3Provider web3Provider)
        {
            _web3Provider = web3Provider;
        }

        public async Task<decimal> GetCirculatingSupply(string contractAddress, BlockchainCode blockchainCode)
        {
            var contractAbi = await GetContractAbi(contractAddress, blockchainCode);
            
            var numberOfDecimals = (short)await _web3Provider.CallFunction<BigInteger>(
                contractAddress,
                contractAbi,
                blockchainCode,
                "decimals"
            );
            
            var totalSupply = _web3Provider.ConvertDenominationToBaseUnit(
                await _web3Provider.CallFunction<BigInteger>(
                    contractAddress,
                    contractAbi,
                    blockchainCode,
                    "totalSupply"
                ),
                numberOfDecimals
            );
            
            var bridgeAccountBalance = _web3Provider.ConvertDenominationToBaseUnit(
                await _web3Provider.CallFunction<BigInteger>(
                    contractAddress,
                    contractAbi,
                    blockchainCode,
                    "balanceOf",
                    Config.BridgeAccountAddress(blockchainCode)
                ),
                numberOfDecimals
            );
            
            return totalSupply - bridgeAccountBalance;
        }

        private static async Task<string> GetContractAbi(string contractAddress, BlockchainCode blockchainCode)
        {
            var response = await new HttpClient().GetAsync($"{Config.AssetBridgeApiUrl}/assets/{MapBlockchainCode(blockchainCode)}/{contractAddress}/abi");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(result);
            return obj["data"].ToString();
        }
        
        private static int MapBlockchainCode(BlockchainCode blockchainCode) =>
            blockchainCode == BlockchainCode.Eth ? 0 : 1;
    }
}