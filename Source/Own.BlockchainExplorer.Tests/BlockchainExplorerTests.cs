using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Tests.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Own.BlockchainExplorer.Tests
{
    public class BlockchainExplorerTests : IntegrationTestsBase
    {
        [Fact]
        public async Task CompareAddressBalances()
        {
            IBlockchainInfoService blockchainInfoService = Instantiate<IBlockchainInfoService>();
            IBlockchainClient blockchainClient = Instantiate<IBlockchainClient>();

            IEnumerable<string> addresses = new List<string>();

            using (var uow = NewUnitOfWork())
            {
                addresses = NewRepository<Address>(uow).GetAs(a => true, a => a.BlockchainAddress);
            }

            foreach (var address in addresses)
            {
                var databaseResult = blockchainInfoService.GetAddressInfo(address);
                Assert.True(databaseResult.Successful, $"Retrieving address {address} from database failed.");

                var blockchainResult = await blockchainClient.GetAddressInfo(address);
                Assert.True(blockchainResult.Successful, $"Retrieving address {address} from blockchain failed.");

                var addressInfoDto = databaseResult.Data;
                var addressBlockchainDto = blockchainResult.Data;

                Assert.Equal(addressBlockchainDto.Balance.Available, addressInfoDto.ChxBalanceInfo.AvailableBalance);
                Assert.Equal(addressBlockchainDto.Balance.Deposit, addressInfoDto.ChxBalanceInfo.ValidatorDeposit);
                Assert.Equal(addressBlockchainDto.Balance.Staked, addressInfoDto.ChxBalanceInfo.DelegatedStakes);
            }
        }

        [Fact]
        public void CompareAccountHoldings()
        {

        }

        [Fact]
        public void CompareAssetHoldings()
        {

        }
    }
}
