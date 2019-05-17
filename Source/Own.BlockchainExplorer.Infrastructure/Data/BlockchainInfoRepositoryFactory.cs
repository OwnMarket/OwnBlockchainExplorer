using Own.BlockchainExplorer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class BlockchainInfoRepositoryFactory : IBlockchainInfoRepositoryFactory
    {
        public IBlockchainInfoRepository Create(IUnitOfWork unitOfWork)
        {
            return new BlockchainInfoRepository((unitOfWork as UnitOfWork).Db);
        }
    }
}
