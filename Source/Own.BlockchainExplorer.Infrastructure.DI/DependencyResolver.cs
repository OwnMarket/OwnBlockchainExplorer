namespace Own.BlockchainExplorer.Infrastructure.DI
{
    /// <summary>
    /// Used to resolve dependencies not managed by DI container.
    /// </summary>
    public static class DependencyResolver
    {
        public static Core.Interfaces.IConfigurationProvider GetConfigurationProvider()
        {
            return new AppSettingsConfigurationProvider();
        }
    }
}
