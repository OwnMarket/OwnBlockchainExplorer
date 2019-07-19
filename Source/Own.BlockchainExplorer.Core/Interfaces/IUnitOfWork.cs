﻿using System;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void BeginTransaction();
        void CommitTransaction();
    }
}
