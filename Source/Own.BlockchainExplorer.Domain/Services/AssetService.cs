using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IAssetBridgeService _assetBridgeService;

        public AssetService(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IAssetBridgeService assetBridgeService) :
            base(unitOfWorkFactory, repositoryFactory)
        {
            _assetBridgeService = assetBridgeService;
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

        public async Task<Result<AssetShortInfoDto>> GetAssetInfo(string assetHash)
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

                var bridgeTransfers = await _assetBridgeService.GetBridgeTransferStats(asset.Hash);

                return Result.Success(new AssetShortInfoDto
                {
                    Hash = asset.Hash,
                    AssetCode = asset.AssetCode,
                    TotalSupply = asset.HoldingsByAssetId.Sum(h => h.Balance ?? 0),
                    HoldersCount = asset.HoldingsByAssetId.Count(h => h.Balance > 0),
                    TransfersCount = events.Count,
                    ControllerAddress = asset.ControllerAddress,
                    BridgeTransferStats = bridgeTransfers.Successful ? bridgeTransfers.Data : null
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
    }
}