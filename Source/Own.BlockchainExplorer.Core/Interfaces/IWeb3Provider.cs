using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IWeb3Provider
    {
        Dictionary<string, object> GetInputParameters(
            string smartContractAddress,
            string abi,
            BlockchainCode code,
            string functionName,
            string inputData);

        Task<T> CallFunction<T>(
            string smartContractAddress,
            string abi,
            BlockchainCode code,
            string functionName,
            params object[] functionInput);
        
        decimal ConvertDenominationToBaseUnit(BigInteger value, int numberOfDecimals);
    }
}