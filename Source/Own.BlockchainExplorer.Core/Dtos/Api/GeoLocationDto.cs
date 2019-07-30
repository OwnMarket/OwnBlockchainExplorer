using Newtonsoft.Json;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class GeoLocationDto
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("continent_code")]
        public string ContinentCode { get; set; }
        [JsonProperty("country_code2")]
        public string CountryCode { get; set; }
        [JsonProperty("country_name")]
        public string CountryName { get; set; }
        [JsonProperty("country_capital")]
        public string CountryCapital { get; set; }
        [JsonProperty("state_prov")]
        public string StateProv { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("latitude")]
        public string Latitude { get; set; }
        [JsonProperty("longitude")]
        public string Longitude { get; set; }
        [JsonProperty("calling_code")]
        public string CallingCode { get; set; }
        [JsonProperty("languages")]
        public string Languages { get; set; }
        [JsonProperty("country_flag")]
        public string CountryFlag { get; set; }
        [JsonProperty("isp")]
        public string Isp { get; set; }
        [JsonProperty("currency")]
        public CurrencyDto Currency { get; set; }
        [JsonProperty("time_zone")]
        public TimeZoneDto TimeZone { get; set; }

        public class CurrencyDto
        {
            [JsonProperty("code")]
            public string Code { get; set; }
        }

        public class TimeZoneDto
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("offset")]
            public string Offset { get; set; }
        }
    }

}
