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

namespace Own.BlockchainExplorer.Scanner
{
    class Program
    {
        private static IScannerService _scannerService;
        private static bool _continueScanning = true;

        static void Main(string[] args)
        {
            ConfigureApp();
            var serviceProvider = ConfigureServices();
            _scannerService = serviceProvider.GetService<IScannerService>();

            TaskScheduler.Run(RunCycle, 1);

            WaitForCancellation();
        }

        private static void ConfigureApp()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
            Log.Initialize($"scanner_{DateTime.UtcNow.ToShortDateString()}.log");
        }

        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

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
            if (_continueScanning)
            {
                _scannerService.InitialBlockchainConfiguration();
                var result = _scannerService.CheckNewBlocks().Result;
                if (result.Failed)
                {
                    var message = string.Join(";", result.Alerts.Select(a => a.Message));
                    Console.WriteLine(message);
                    _continueScanning = false;
                }
            }     
        }
    }
}
