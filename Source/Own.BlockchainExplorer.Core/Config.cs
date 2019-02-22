using System.IO;
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

        public static string DB => _provider.GetString("appSettings:DB");
        public static string ContentRoot => Directory.GetCurrentDirectory();
    }
}
