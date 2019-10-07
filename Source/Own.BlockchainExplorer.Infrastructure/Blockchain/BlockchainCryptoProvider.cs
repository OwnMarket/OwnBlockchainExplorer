using Own.Blockchain.Public.Crypto;
using Own.Blockchain.Public.Core.DomainTypes;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Blockchain
{
    public class BlockchainCryptoProvider : IBlockchainCryptoProvider
    {
        public string DeriveHash(string address, long nonce, short actionNumber)
        {
            return Hashing.deriveHash(
                BlockchainAddress.NewBlockchainAddress(address),
                Nonce.NewNonce(nonce),
                TxActionNumber.NewTxActionNumber(actionNumber));
        }
    }
}
