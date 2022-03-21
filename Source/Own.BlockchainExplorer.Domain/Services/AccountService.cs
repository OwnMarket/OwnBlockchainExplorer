using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class AccountService : DataService, IAccountService
    {
        public AccountService(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory) :
            base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<List<AccountShortInfoDto>> GetAccounts(int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var accounts = NewRepository<Account>(uow).GetAll(
                    a => a.HoldingsByAccountId
                ).Skip((page - 1) * limit).Take(limit).ToList();
                
                return Result.Success(accounts.Select(a => new AccountShortInfoDto
                {
                    Hash = a.Hash,
                    AssetsCount = a.HoldingsByAccountId.Select(h => h.AssetHash).Distinct().Count(),
                    ControllerAddress = a.ControllerAddress
                }).ToList());
            }
        }

        public Result<AccountShortInfoDto> GetAccountInfo(string accountHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var account = NewRepository<Account>(uow).Get(
                    a => a.Hash == accountHash,
                    a => a.HoldingsByAccountId
                ).FirstOrDefault();
                if (account is null) return Result.Failure<AccountShortInfoDto>("Account not found.");

                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.AccountId == account.AccountId && e.Fee != 0 &&
                    e.TxAction.ActionType == ActionType.TransferAsset.ToString(),
                    e => e.TxAction
                ).ToList();
                
                return Result.Success(new AccountShortInfoDto
                {
                    Hash = account.Hash,
                    AssetsCount = account.HoldingsByAccountId.Select(h => h.AssetHash).Distinct().Count(),
                    ControllerAddress = account.ControllerAddress,
                    TransfersCount = events.Count
                });
            }
        }
        
        public Result<List<AccountTransfersInfoDto>> GetAccountTransfers(string accountHash, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var account = NewRepository<Account>(uow).Get(a => a.Hash == accountHash).FirstOrDefault();
                if (account is null) return Result.Failure<List<AccountTransfersInfoDto>>("Account not found.");

                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.AccountId == account.AccountId && e.Fee != 0 && 
                    e.TxAction.ActionType == ActionType.TransferAsset.ToString(),
                    e => e.TxAction,
                    e => e.Tx,
                    e => e.Asset
                ).Skip((page - 1) * limit).Take(limit).ToList();
                
                var transfers = events.Select(e => new
                {
                    Transfer = JsonConvert.DeserializeObject<TransferAssetData>(e.TxAction.ActionData),
                    e.Asset.AssetCode,
                    e.Tx.Hash,
                    Date = e.Tx.DateTime
                }).ToList();
                
                return Result.Success(transfers.Select(t => new AccountTransfersInfoDto
                {
                    Hash = t.Hash,
                    FromAccountHash = t.Transfer.FromAccountHash,
                    ToAccountHash = t.Transfer.ToAccountHash,
                    AssetHash = t.Transfer.AssetHash,
                    AssetCode = t.AssetCode,
                    Amount = t.Transfer.Amount,
                    Date = t.Date
                }).ToList());
            }
        }
        
        public Result<List<AccountHoldingInfoDto>> GetAccountHoldings(string accountHash, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var account = NewRepository<Account>(uow).Get(a => a.Hash == accountHash).FirstOrDefault();
                if (account is null) return Result.Failure<List<AccountHoldingInfoDto>>("Account not found.");

                var holdings = NewRepository<Holding>(uow).Get(
                    h => h.AccountId == account.AccountId,
                    h => h.Asset
                ).Skip((page - 1) * limit).Take(limit).ToList();

                return Result.Success(holdings.Select(h => new AccountHoldingInfoDto
                {
                    AssetHash = h.Asset.Hash,
                    AssetCode = h.Asset.AssetCode,
                    Balance = h.Balance
                }).ToList());
            }
        }
    }
}