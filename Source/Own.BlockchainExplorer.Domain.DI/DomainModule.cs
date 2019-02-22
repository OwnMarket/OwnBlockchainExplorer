using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.DI
{
    public class DomainModule
    {
        public void Load(IServiceCollection serviceCollection)
        {
            var domainAssembly = typeof(DataService).Assembly;

            domainAssembly.GetTypes()
                .Where(t => !t.GetTypeInfo().IsAbstract && t.Name.EndsWith("Service"))
                .Each(implementationType =>
                    implementationType.GetInterfaces().Each(interfaceType =>
                        serviceCollection.AddTransient(interfaceType, implementationType)));
        }
    }
}
