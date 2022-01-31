using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Infrastructure.Blockchain;
using Own.BlockchainExplorer.Infrastructure.Data;
using Own.BlockchainExplorer.Infrastructure.Geo;

namespace Own.BlockchainExplorer.Infrastructure.DI
{
    public class InfrastructureModule
    {
        public void Load(IServiceCollection serviceCollection)
        {
            var infrastructureAssembly = typeof(UnitOfWork).Assembly;

            infrastructureAssembly.GetTypes()
                .Where(t => !t.GetTypeInfo().IsAbstract && t.Name.EndsWith("Factory"))
                .Each(implementationType =>
                    implementationType.GetInterfaces().Each(interfaceType =>
                        serviceCollection.AddTransient(interfaceType, implementationType)));

            serviceCollection.AddTransient<IBlockchainClient>(p => new BlockchainClient(Config.NodeApi));
            serviceCollection.AddTransient<IGeoLocationService, GeoLocationService>();
            serviceCollection.AddTransient<IBlockchainCryptoProvider, BlockchainCryptoProvider>();
            serviceCollection.AddTransient<IWeb3Provider, NethereumProvider>();
            serviceCollection.AddTransient<IAssetBridgeRepository, AssetBridgeRepository>();
        }
    }
}
