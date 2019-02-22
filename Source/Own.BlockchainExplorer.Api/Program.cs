using System;
using Microsoft.AspNetCore.Hosting;
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
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
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
