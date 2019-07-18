using Newtonsoft.Json;
using Own.BlockchainExplorer.Common;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Own.BlockchainExplorer.Infrastructure.Blockchain
{
    public class BlockchainClient : IBlockchainClient
    {
        private readonly string _nodeApiUrl;
        private HttpClient _httpClient;

        public BlockchainClient(string nodeApiUrl)
        {
            _nodeApiUrl = nodeApiUrl;
        }

        private HttpClient HttpClient
        {
            get
            {
                if (_httpClient == default(HttpClient))
                {
                    _httpClient = new HttpClient();
                }

                return _httpClient;
            }
        }

        private async Task<Result<T>> HandleBlockChainResponse<T>(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                /*Log.Error(
                    "{0} {1} [{2}] {3}".F(
                        responseMessage.RequestMessage?.Method,
                        responseMessage.RequestMessage?.RequestUri,
                        (int)responseMessage.StatusCode,
                        responseMessage.ReasonPhrase)
                    .Trim());*/
            }

            responseMessage.EnsureSuccessStatusCode();
            var json = await responseMessage.Content.ReadAsStringAsync();
            //Log.Debug("BLOCKCHAIN NODE RESPONSE: {0}", json);

            try
            {
                return Result.Success(JsonConvert.DeserializeObject<T>(json));
            }
            catch (Exception)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponseDto>(json);
                var alerts = error.Errors?.Select(e => Alert.Error(e));
                return Result.Failure<T>(alerts);
            }
        }

        private HttpContent JsonContent<T>(T item)
        {
            var jsonString = JsonConvert.SerializeObject(item);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        public async Task<Result<AddressDto>> GetAddressInfo(string blockchainAddress)
        {
            var url = $"{_nodeApiUrl}/address/{blockchainAddress}";
            var result = await HttpClient.GetAsync(url);
            return await HandleBlockChainResponse<AddressDto>(result);
        }

        public async Task<Result<AccountDto>> GetAccountInfo(string accountHash, string assetHash = null)
        {
            var url = string.Concat($"{_nodeApiUrl}/account/{accountHash}", !string.IsNullOrEmpty(assetHash) ? $"?asset={assetHash}" : "");
            var result = await HttpClient.GetAsync(url);
            return await HandleBlockChainResponse<AccountDto>(result);
        }

        public async Task<Result<BlockDto>> GetBlockInfo(long blockNumber)
        {
            var url = $"{_nodeApiUrl}/block/{blockNumber}";
            var result = await HttpClient.GetAsync(url);
            return await HandleBlockChainResponse<BlockDto>(result);
        }

        public async Task<Result<TxDto>> GetTxInfo(string txHash)
        {
            var url = $"{_nodeApiUrl}/tx/{txHash}";
            var result = await HttpClient.GetAsync(url);
            return await HandleBlockChainResponse<TxDto>(result);
        }

        public async Task<Result<EquivocationDto>> GetEquivocationInfo(string equivocationProofHash)
        {
            var url = $"{_nodeApiUrl}/equivocation/{equivocationProofHash}";
            var result = await HttpClient.GetAsync(url);
            return await HandleBlockChainResponse<EquivocationDto>(result);
        }


    }
}
