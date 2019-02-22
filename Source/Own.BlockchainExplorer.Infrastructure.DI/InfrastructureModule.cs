using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Infrastructure.Data;

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
        }
    }
}
