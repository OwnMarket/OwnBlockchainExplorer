using System;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Own.BlockchainExplorer.Common;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Infrastructure.DI;

namespace Own.BlockchainExplorer.Api
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureApp();
            StartApi();
        }

        private static void ConfigureApp()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
            Log.Initialize($"scanner_{DateTime.UtcNow.IsoDateString()}.log");
        }

        private static void StartApi()
        {
            var builder = new WebHostBuilder();
            builder
                .UseContentRoot(Config.ContentDir)
                .UseUrls(Config.ApiUrls)
                .UseKestrel()
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
