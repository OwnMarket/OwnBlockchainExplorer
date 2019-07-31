using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Geo
{
    public class GeoLocationProvider : IGeoLocationProvider
    {
        private readonly string _ipGeoApiUrl;
        private HttpClient _httpClient;

        public GeoLocationProvider(string ipGeoApiUrl)
        {
            _ipGeoApiUrl = ipGeoApiUrl;
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

        public async Task<Result<GeoLocationDto>> GetGeoLocation(string ipAddress)
        {
            var url = $"{_ipGeoApiUrl}?apiKey={Config.GeoApiKey}&ip={ipAddress}";
            var result = await HttpClient.GetAsync(url);
            return await HandleIpGeoApiResponse<GeoLocationDto>(result);
        }

        private async Task<Result<T>> HandleIpGeoApiResponse<T>(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Error(
                    "{0} {1} [{2}] {3}".F(
                        responseMessage.RequestMessage?.Method,
                        responseMessage.RequestMessage?.RequestUri,
                        (int)responseMessage.StatusCode,
                        responseMessage.ReasonPhrase)
                    .Trim());
            }

            responseMessage.EnsureSuccessStatusCode();
            var json = await responseMessage.Content.ReadAsStringAsync();

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
    }
}
