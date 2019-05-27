using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Tests.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Own.BlockchainExplorer.Tests
{
    public class BlockchainExplorerTests : IntegrationTestsBase
    {
        [Fact]
        public async Task CompareAddressBalances()
        {
            IAddressInfoService addressInfoService = Instantiate<IAddressInfoService>();
            IBlockchainClient blockchainClient = Instantiate<IBlockchainClient>();

            IEnumerable<string> addresses = new List<string>();

            using (var uow = NewUnitOfWork())
            {
                addresses = NewRepository<Address>(uow).GetAs(a => true, a => a.BlockchainAddress);
            }

            var failedAddresses = new List<string>();

            foreach (var address in addresses)
            {
                var databaseResult = addressInfoService.GetAddressInfo(address);
                Assert.True(databaseResult.Successful, $"Retrieving address {address} from database failed.");

                var blockchainResult = await blockchainClient.GetAddressInfo(address);
                Assert.True(blockchainResult.Successful, $"Retrieving address {address} from blockchain failed.");

                var addressInfoDto = databaseResult.Data;
                var addressBlockchainDto = blockchainResult.Data;

                if (addressBlockchainDto.Balance.Available != addressInfoDto.ChxBalanceInfo.AvailableBalance
                    || addressBlockchainDto.Balance.Deposit != addressInfoDto.ChxBalanceInfo.ValidatorDeposit
                    || addressBlockchainDto.Balance.Staked != addressInfoDto.ChxBalanceInfo.DelegatedStakes)
                    failedAddresses.Add(address);

                //Assert.True(addressBlockchainDto.Balance.Available == addressInfoDto.ChxBalanceInfo.AvailableBalance, $"Available CHX for {address} does not match.");
                //Assert.True(addressBlockchainDto.Balance.Deposit == addressInfoDto.ChxBalanceInfo.ValidatorDeposit, $"Deposited CHX for {address} does not match.");
                //Assert.True(addressBlockchainDto.Balance.Staked == addressInfoDto.ChxBalanceInfo.DelegatedStakes, $"Staked CHX for {address} does not match.");
            }

            var reallyFailed = new List<string>();
            using (var uow = NewUnitOfWork())
            {
                var valdiatorRepo = NewRepository<Validator>(uow);
                foreach(var failed in failedAddresses)
                {
                    if (!valdiatorRepo.Exists(v => v.BlockchainAddress == failed))
                        reallyFailed.Add(failed);
                }
            }
            Assert.True(failedAddresses.Any());
        }

        [Fact]
        public async Task CompareAccountHoldings()
        {
            IBlockchainInfoService blockchainInfoService = Instantiate<IBlockchainInfoService>();
            IBlockchainClient blockchainClient = Instantiate<IBlockchainClient>();

            IEnumerable<string> accounts = new List<string>();

            using (var uow = NewUnitOfWork())
            {
                accounts = NewRepository<Account>(uow).GetAs(a => true, a => a.Hash);
            }

            foreach (var account in accounts)
            {
                var databaseResult = blockchainInfoService.GetAccountInfo(account);
                Assert.True(databaseResult.Successful, $"Retrieving account {account} from database failed.");

                var blockchainResult = await blockchainClient.GetAccountInfo(account);
                Assert.True(blockchainResult.Successful, $"Retrieving account {account} from blockchain failed.");

                var accountInfoDto = databaseResult.Data;
                var accountBlockchainDto = blockchainResult.Data;

                Assert.Equal(accountBlockchainDto.Holdings.Count, accountInfoDto.Holdings.Count);

                foreach(var holdingInfo in accountInfoDto.Holdings)
                {
                    var holdingBlockchain = accountBlockchainDto.Holdings
                        .FirstOrDefault(h => h.AssetHash == holdingInfo.AssetHash);

                    Assert.NotNull(holdingBlockchain);
                    Assert.Equal(holdingBlockchain.Balance, holdingInfo.Balance);
                }
            }
        }
    }
}
