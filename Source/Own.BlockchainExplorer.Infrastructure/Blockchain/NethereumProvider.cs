using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Blockchain
{
    public class NethereumProvider : IWeb3Provider
    {
        public Dictionary<string, object> GetInputParameters(
            string smartContractAddress,
            string abi,
            BlockchainCode code,
            string functionName, 
            string inputData)
        {
            var web3 = new Web3(Config.NodeUrl(code));
            var contract = web3.Eth.GetContract(abi, smartContractAddress);
            var parameters = contract.GetFunction(functionName).DecodeInput(inputData);
            return parameters?.ToDictionary(p => p.Parameter.Name, p => p.Result);           
        }
        
        public async Task<T> CallFunction<T>(
            string smartContractAddress,
            string abi,
            BlockchainCode code,
            string functionName,
            params object[] functionInput)
        {
            var web3 = new Web3(Config.NodeUrl(code));
            var contract = web3.Eth.GetContract(abi, smartContractAddress);
            return await contract.GetFunction(functionName).CallAsync<T>(functionInput);
        }
        
        public decimal ConvertDenominationToBaseUnit(BigInteger value, int numberOfDecimals) =>
            Web3.Convert.FromWei(value, numberOfDecimals);
    }
}