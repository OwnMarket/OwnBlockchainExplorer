using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Infrastructure.Data.EF;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private static readonly LoggerFactory LoggerFactory =
            new LoggerFactory(new[] {new ConsoleLoggerProvider((_, __) => true, true)});

        public IUnitOfWork Create(UnitOfWorkMode mode = UnitOfWorkMode.ReadOnly)
        {
            var options = new DbContextOptionsBuilder()
#if DEBUG
                .UseLoggerFactory(LoggerFactory)
                .EnableSensitiveDataLogging()
#endif
                .UseNpgsql(Config.DB)
                .Options;

            var db = new OwnDb(options);

            db.ChangeTracker.LazyLoadingEnabled = false;

            if (mode == UnitOfWorkMode.ReadOnly)
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;
                //db.Configuration.ProxyCreationEnabled = false;
            }

            return new UnitOfWork(db, mode);
        }
    }
}
