using System;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Infrastructure.Data.EF;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UnitOfWorkMode _mode;
        internal OwnDb Db { get; }

        public UnitOfWork(OwnDb db, UnitOfWorkMode mode)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
            _mode = mode;
        }

        public void Commit()
        {
            if (_mode == UnitOfWorkMode.ReadOnly)
                throw new InvalidOperationException("Commit is not allowed in read-only UnitOfWork.");

            Db.SaveChanges();
        }

        public void Dispose()
        {
            Db.Dispose();
        }
    }
}
