using Own.Blockchain.Common;
using Own.Blockchain.Public.Crypto;
using Own.BlockchainExplorer.Core.Interfaces;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Infrastructure.Blockchain
{
    public class BlockchainCryptoProvider : IBlockchainCryptoProvider
    {
        public string DeriveHash(string address, long nonce, short actionNumber)
        {

            var addressHash = Hashing.decode(address);
            var nonceArray = Conversion.int64ToBytes(nonce);
            var actionArray = Conversion.int16ToBytes(actionNumber);

            var fullHash = new List<byte>();
            fullHash.AddRange(addressHash);
            fullHash.AddRange(nonceArray);
            fullHash.AddRange(actionArray);

            return Hashing.hash(fullHash.ToArray());
        }
    }
}
