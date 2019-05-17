using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Domain.DI;
using Own.BlockchainExplorer.Infrastructure.DI;
using System;
using System.Linq;

namespace Own.BlockchainExplorer.Scanner
{
    class Program
    {
        private static IScannerService _scannerService;

        static void Main(string[] args)
        {
            ConfigureApp();
            var serviceProvider = ConfigureServices();
            _scannerService = serviceProvider.GetService<IScannerService>();

            RunCycle().GetAwaiter().GetResult();
        }

        private static void ConfigureApp()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
        }

        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            new InfrastructureModule().Load(serviceCollection);
            new DomainModule().Load(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        private static async Task RunCycle()
        {
            _scannerService.InitialBlockchainConfiguration();
            var result = await _scannerService.CheckNewBlocks();
            if (result.Failed)
                Console.WriteLine(string.Join(";", result.Alerts.Select(a => a.Message))); 
        }
    }
}
