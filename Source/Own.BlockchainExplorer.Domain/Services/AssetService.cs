using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class AssetService : DataService, IAssetService
    {
        public AssetService(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory) :
            base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<List<AssetShortInfoDto>> GetAssets(string accountHash, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                if (accountHash.IsNullOrEmpty())
                {
                    var assets = NewRepository<Asset>(uow).GetAll(
                        a => a.HoldingsByAssetId
                    ).Skip((page - 1) * limit).Take(limit).ToList();

                    return Result.Success(assets.Select(a => new AssetShortInfoDto
                    {
                        Hash = a.Hash,
                        AssetCode = a.AssetCode,
                        TotalSupply = a.HoldingsByAssetId.Sum(h => h.Balance ?? 0),
                        HoldersCount = a.HoldingsByAssetId.Count(h => h.Balance > 0)
                    }).ToList());
                }

                var account = NewRepository<Account>(uow).GetDeep(
                    a => a.Hash == accountHash,
                    a => a.HoldingsByAccountId,
                    h => h.Asset
                ).FirstOrDefault();

                return Result.Success(account?.HoldingsByAccountId.Select(h => h.Asset).Select(a => new AssetShortInfoDto
                {
                    Hash = a.Hash,
                    AssetCode = a.AssetCode,
                    TotalSupply = a.HoldingsByAssetId.Sum(h => h.Balance ?? 0),
                    HoldersCount = a.HoldingsByAssetId.Count(h => h.Balance > 0)
                }).ToList());
            }
        }

        public Result<AssetShortInfoDto> GetAssetInfo(string assetHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var asset = NewRepository<Asset>(uow).Get(
                    a => a.Hash == assetHash,
                    a => a.HoldingsByAssetId
                ).FirstOrDefault();
                if (asset is null) return Result.Failure<AssetShortInfoDto>("Asset not found.");

                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.AssetId == asset.AssetId && e.Fee != 0 && 
                    e.TxAction.ActionType == ActionType.TransferAsset.ToString(),
                    e => e.TxAction
                ).ToList();

                return Result.Success(new AssetShortInfoDto
                {
                    Hash = asset.Hash,
                    AssetCode = asset.AssetCode,
                    TotalSupply = asset.HoldingsByAssetId.Sum(h => h.Balance ?? 0),
                    HoldersCount = asset.HoldingsByAssetId.Count(h => h.Balance > 0),
                    TransfersCount = events.Count,
                    ControllerAddress = asset.ControllerAddress
                });
            }
        }
        
        public Result<List<AssetTransferInfoDto>> GetAssetTransfers(string assetHash, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var asset = NewRepository<Asset>(uow).Get(a => a.Hash == assetHash).FirstOrDefault();
                if (asset is null) return Result.Failure<List<AssetTransferInfoDto>>("Asset not found.");

                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.AssetId == asset.AssetId && e.Fee != 0 && 
                    e.TxAction.ActionType == ActionType.TransferAsset.ToString(),
                    e => e.TxAction,
                    e => e.Tx
                ).Skip((page - 1) * limit).Take(limit).ToList();
                
                var transfers = events.Select(e => new
                {
                    Transfer = JsonConvert.DeserializeObject<TransferAssetData>(e.TxAction.ActionData),
                    e.Tx.Hash,
                    Date = e.Tx.DateTime
                }).ToList();
                
                return Result.Success(transfers.Select(t => new AssetTransferInfoDto
                {
                    Hash = t.Hash,
                    FromAccountHash = t.Transfer.FromAccountHash,
                    ToAccountHash = t.Transfer.ToAccountHash,
                    AssetHash = t.Transfer.AssetHash,
                    Amount = t.Transfer.Amount,
                    Date = t.Date
                }).ToList());
            }
        }
        
        public Result<List<AssetHolderInfoDto>> GetAssetHolders(string assetHash, int page, int limit)
        {
            using (var uow = NewUnitOfWork())
            {
                var asset = NewRepository<Asset>(uow).Get(a => a.Hash == assetHash).FirstOrDefault();
                if (asset is null) return Result.Failure<List<AssetHolderInfoDto>>("Asset not found.");

                var holdings = NewRepository<Holding>(uow).Get(
                    h => h.AssetId == asset.AssetId
                ).Skip((page - 1) * limit).Take(limit).ToList();

                return Result.Success(holdings.Select(h => new AssetHolderInfoDto
                {
                    AccountHash = h.AccountHash,
                    Balance = h.Balance
                }).ToList());
            }
        }

        public void FixIncorrectAssetHoldings()
        {
            using (var uow = NewUnitOfWork(UnitOfWorkMode.Writable))
            {
                var repo = NewRepository<Holding>(uow);
                
                var eventData = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.TxAction.ActionType == ActionType.CreateAssetEmission.ToString(),
                    e => e.TxAction
                ).Where(e => e.AssetId.HasValue).Select(e =>
                    JsonConvert.DeserializeObject<CreateAssetEmissionData>(e.TxAction.ActionData)
                ).GroupBy(g => g.AssetHash).Select(g => new
                {
                    AssetHash = g.Key,
                    AccountHash = g.First().EmissionAccountHash,
                    Amount = g.Sum(d => d.Amount)
                }).ToList();
                
                var assetHashes = eventData.Select(e => e.AssetHash).ToList();
                var holdings = repo.Get(h => assetHashes.Contains(h.AssetHash)).ToList();

                var reservedHoldings = holdings.GroupBy(h => h.AssetHash).Select(g => new
                {
                    AssetHash = g.Key,
                    Amount = g.Where(h => h.Balance.HasValue).Sum(h => h.Balance)
                }).ToList();
                
                foreach (var data in eventData)
                {
                    var holding = holdings.FirstOrDefault(h => h.AccountHash == data.AccountHash && h.AssetHash == data.AssetHash);
                    var reservedHolding = reservedHoldings.FirstOrDefault(h => h.AssetHash == data.AssetHash)?.Amount ?? 0;
                    if (holding != null && !holding.Balance.HasValue)
                    {
                        holding.Balance = data.Amount - reservedHolding;
                        repo.Update(holding);
                    }
                }
                
                uow.Commit();
            }
        }
    }
}