using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IBlockchainCryptoProvider
    {
        string DeriveHash(string address, long nonce, short actionNumber);
    }
}
