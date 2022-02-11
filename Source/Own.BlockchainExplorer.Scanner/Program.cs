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
using Own.BlockchainExplorer.Common;
using Own.BlockchainExplorer.Common.Extensions;

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
            
            Task.Run(async () =>
            {
                while (true)
                {
                    RunCycle();
                    await Task.Delay(1000);
                }
            });

            WaitForCancellation();
        }

        private static void ConfigureApp()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
            Log.Initialize($"scanner_{DateTime.UtcNow.IsoDateString()}.log");
        }

        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddMemoryCache();
            new InfrastructureModule().Load(serviceCollection);
            new DomainModule().Load(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        private static void WaitForCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => cancellationTokenSource.Cancel();
            Console.CancelKeyPress += (s, e) => cancellationTokenSource.Cancel();
            while (!cancellationTokenSource.Token.IsCancellationRequested)
                Thread.Sleep(5000);
        }

        private static void RunCycle()
        {
            _scannerService.InitialBlockchainConfiguration();
            var result = _scannerService.CheckNewBlocks().Result;
            if (result.Failed)
            {
                var message = string.Join(";", result.Alerts.Select(a => a.Message));
                Log.Error(message);
            }
        }
    }
}
