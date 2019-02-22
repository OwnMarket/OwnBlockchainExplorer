using System;
using Microsoft.Extensions.DependencyInjection;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Domain.DI;
using Own.BlockchainExplorer.Infrastructure.DI;

namespace Own.BlockchainExplorer.Scanner
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureApp();
            var serviceProvider = ConfigureServices();
        }

        private static void ConfigureApp()
        {
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
        }

        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            new InfrastructureModule().Load(serviceCollection);
            new DomainModule().Load(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }
    }
}
