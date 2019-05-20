using Microsoft.Extensions.DependencyInjection;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Domain.DI;
using Own.BlockchainExplorer.Infrastructure.Data;
using Own.BlockchainExplorer.Infrastructure.DI;
using System;

namespace Own.BlockchainExplorer.Tests.Common
{
    public abstract class IntegrationTestsBase : IDisposable
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        private ServiceProvider _serviceProvider;

        protected IntegrationTestsBase()
        {
            Config.SetConfigurationProvider(DependencyResolver.GetConfigurationProvider());
            _unitOfWorkFactory = new UnitOfWorkFactory();
            _repositoryFactory = new RepositoryFactory();
        }

        protected IUnitOfWork NewUnitOfWork(UnitOfWorkMode mode = UnitOfWorkMode.ReadOnly)
        {
            return _unitOfWorkFactory.Create(mode);
        }

        protected IRepository<T> NewRepository<T>(IUnitOfWork unitOfWork)
            where T : class
        {
            return _repositoryFactory.Create<T>(unitOfWork);
        }

        protected T Instantiate<T>()
        {
            if (_serviceProvider == null)
                BuildServiceProvider();

            return _serviceProvider.GetRequiredService<T>();
        }

        private void BuildServiceProvider()
        {
            if (_serviceProvider != null)
                throw new InvalidOperationException("ServiceProvider is already built.");

            var serviceCollection = new ServiceCollection();
            new DomainModule().Load(serviceCollection);
            new InfrastructureModule().Load(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public void Dispose()
        {
            if (_serviceProvider != null)
                _serviceProvider.Dispose();
        }

    }
}
