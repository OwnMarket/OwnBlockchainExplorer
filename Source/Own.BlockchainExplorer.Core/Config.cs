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

        // API
        public static string[] ApiUrls => _provider.GetString("server.urls")?.Split(',', ';');
        public static string[] AccessControlAllowOrigins => _provider.GetString("cors:allowOrigins").Split(',', ';');
        public static string[] AccessControlAllowMethods => _provider.GetString("cors:allowMethods").Split(',', ';');
        public static string[] AccessControlAllowHeaders => _provider.GetString("cors:allowHeaders").Split(',', ';');
        public static string[] AccessControlExposeHeaders => _provider.GetString("cors:exposeHeaders").Split(',', ';');
    }
}
