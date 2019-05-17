using Own.BlockchainExplorer.Common.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IScannerService
    {
        Task<Result> CheckNewBlocks();
        Result InitialBlockchainConfiguration();
    }
}
