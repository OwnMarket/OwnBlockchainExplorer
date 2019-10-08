using System.IO;
using System.Reflection;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Core
{
    public static class Config
    {
        private static IConfigurationProvider _provider;
        public static void SetConfigurationProvider(IConfigurationProvider provider)
        {
            _provider = provider;
        }

        public static string AppDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ContentDir => Directory.GetCurrentDirectory();
        public static string DB => _provider.GetString("appSettings:DB");

        // Blockchain
        public static string NodeApi => _provider.GetString("nodeApiUrl");
        public static string GenesisAddress => _provider.GetString("genesisAddress");
        public static decimal? GenesisChxSupply => _provider.GetDecimal("genesisChxSupply");
        public static string[] GenesisValidators => _provider.GetString("genesisValidators").Split(',', ';');
        public static string FakeValidator => _provider.GetString("fakeValidator");
        public static string[] GenesisAddresses => _provider.GetString("genesisAddresses").Split(',', ';');

        // GeoLocation
        public static string IpGeoApi => _provider.GetString("geo:apiUrl");
        public static string GeoApiKey => _provider.GetString("geo:apiKey");
        public static int GeoCacheTime => _provider.GetInteger("geoCacheTime").Value; // Minutes
       
        public static int ScanBatchSize => _provider.GetInteger("scanBatchSize").Value;

        // API
        public static string[] ApiUrls => _provider.GetString("server.urls")?.Split(',', ';');
        public static string[] AccessControlAllowOrigins => _provider.GetString("cors:allowOrigins").Split(',', ';');
        public static string[] AccessControlAllowMethods => _provider.GetString("cors:allowMethods").Split(',', ';');
        public static string[] AccessControlAllowHeaders => _provider.GetString("cors:allowHeaders").Split(',', ';');
        public static string[] AccessControlExposeHeaders => _provider.GetString("cors:exposeHeaders").Split(',', ';');

        public static string SwaggerXmlDocPath => _provider.GetString("swagger:xmlDocPath");
    }
}
