using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainInfoRepositoryFactory
    {
        IBlockchainInfoRepository Create(IUnitOfWork unitOfWork);
    }
}
