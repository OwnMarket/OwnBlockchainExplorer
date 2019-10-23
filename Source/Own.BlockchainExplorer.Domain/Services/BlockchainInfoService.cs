using System;
using System.Linq;
using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class BlockchainInfoService : DataService, IBlockchainInfoService
    {
        private readonly IBlockchainInfoRepositoryFactory _blockchainInfoRepositoryFactory;

        public BlockchainInfoService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory,
            IBlockchainInfoRepositoryFactory blockchainInfoRepositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
            _blockchainInfoRepositoryFactory = blockchainInfoRepositoryFactory;
        }

        public Result<EquivocationInfoDto> GetEquivocationInfo(string equivocationProofHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Equivocation.EquivocationProofHash == equivocationProofHash,
                    e => e.Equivocation,
                    e => e.Address);
                if (!events.Any()) return Result.Failure<EquivocationInfoDto>(
                    "Equivocation {0} does not exist.".F(equivocationProofHash));

                var equivocation = events.First().Equivocation;
                var eqDto = EquivocationInfoDto.FromDomainModel(equivocation);

                var blockNumber = NewRepository<Block>(uow)
                    .GetAs(b => b.BlockId == equivocation.BlockId, b => b.BlockNumber)
                    .SingleOrDefault();
                eqDto.IncludedInBlockNumber = blockNumber;

                var depositTakenEvent = events.Single(e => e.EventType == EventType.DepositTaken.ToString());
                eqDto.TakenDeposit = new DepositDto
                {
                    BlockchainAddress = depositTakenEvent.Address.BlockchainAddress,
                    Amount = depositTakenEvent.Amount.Value * -1
                };

                eqDto.GivenDeposits = events
                    .Where(e => e.EventType == EventType.DepositGiven.ToString())
                    .Select(e => new DepositDto
                    {
                        BlockchainAddress = e.Address.BlockchainAddress,
                        Amount = e.Amount.Value
                    }).ToList();

                return Result.Success(eqDto);
            }
        }

        public Result<AccountInfoDto> GetAccountInfo(string accountHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Account.Hash == accountHash,
                    e => e.TxAction,
                    e => e.Address,
                    e => e.Account.HoldingEligibilitiesByAccountId);

                if (!events.Any())
                    return Result.Failure<AccountInfoDto>("Account {0} does not exist.".F(accountHash));

                var account = events.First().Account;

                var accountDto = AccountInfoDto.FromDomainModel(account);

                accountDto.Holdings = account.HoldingEligibilitiesByAccountId
                    .Where(h => h.Balance.HasValue)
                    .Select(h => new HoldingDto { AssetHash = h.AssetHash, Balance = h.Balance.Value })
                    .ToList();

                accountDto.Eligibilities = account.HoldingEligibilitiesByAccountId
                    .Where(h => h.IsPrimaryEligible.HasValue || h.KycControllerAddress != null)
                    .Select(h => new EligibilityDto {
                        AssetHash = h.AssetHash,
                        IsPrimaryEligible = h.IsPrimaryEligible,
                        IsSecondaryEligible = h.IsSecondaryEligible,
                        KycControllerAddress = h.KycControllerAddress })
                    .ToList();

                accountDto.ControllerAddresses = events
                    .Where(e => e.TxAction.ActionType == ActionType.CreateAccount.ToString()
                        || e.TxAction.ActionType == ActionType.SetAccountController.ToString())
                    .Select(e => e.Address.BlockchainAddress).Distinct()
                    .Select(s => new ControllerAddressDto { BlockchainAddress = s }).ToList();

                return Result.Success(accountDto);
            }
        }

        public Result<AssetInfoDto> GetAssetInfo(string assetHash)
        {
            using (var uow = NewUnitOfWork())
            {
                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.Asset.Hash == assetHash,
                    e => e.TxAction,
                    e => e.Address,
                    e => e.Asset.HoldingEligibilitiesByAssetId);

                if (!events.Any())
                    return Result.Failure<AssetInfoDto>("Asset {0} does not exist.".F(assetHash));

                var asset = events.First().Asset;

                var assetDto = AssetInfoDto.FromDomainModel(asset);

                assetDto.Holdings = asset.HoldingEligibilitiesByAssetId
                    .Where(h => h.Balance.HasValue)
                    .Select(h => new HoldingDto { AccountHash = h.AccountHash, Balance = h.Balance.Value })
                    .ToList();

                assetDto.Eligibilities = asset.HoldingEligibilitiesByAssetId
                    .Where(h => h.IsPrimaryEligible.HasValue || h.KycControllerAddress != null)
                    .Select(h => new EligibilityDto
                    {
                        AccountHash = h.AccountHash,
                        IsPrimaryEligible = h.IsPrimaryEligible,
                        IsSecondaryEligible = h.IsSecondaryEligible,
                        KycControllerAddress = h.KycControllerAddress
                    })
                    .ToList();

                assetDto.ControllerAddresses = events
                    .Where(e => e.TxAction.ActionType == ActionType.CreateAsset.ToString()
                        || e.TxAction.ActionType == ActionType.SetAssetController.ToString())
                    .Select(e => e.Address.BlockchainAddress).Distinct()
                    .Select(s => new ControllerAddressDto { BlockchainAddress = s }).ToList();

                return Result.Success(assetDto);
            }
        }

        public Result<IEnumerable<TxInfoShortDto>> GetTxs(int limit, int page)
        {
            using (var uow = NewUnitOfWork())
            {
                var txs = _blockchainInfoRepositoryFactory.Create(uow).GetTxs(limit, page);
                var txIds = txs.Select(t => t.TransactionId);

                var events = NewRepository<BlockchainEvent>(uow).Get(
                    e => e.TransactionId.HasValue && txIds.Contains(e.TransactionId.Value),
                    e => e.Block,
                    e => e.Address);

                return Result.Success(txs.Select(tx => new TxInfoShortDto
                {
                    Hash = tx.Hash,
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(tx.Timestamp).UtcDateTime,
                    Status = tx.Status,
                    NumberOfActions = events
                        .Where(e => e.TransactionId == tx.TransactionId).GroupBy(e => e.TxActionId).Count(),
                    SenderAddress = events
                        .Where(e => e.TransactionId == tx.TransactionId && e.Fee.HasValue)
                        .First().Address.BlockchainAddress,
                    BlockNumber = events.Where(e => e.TransactionId == tx.TransactionId).First().Block.BlockNumber
                }));
            }
        }

        public Result<IEnumerable<BlockInfoShortDto>> GetBlocks(int limit, int page)
        {
            using (var uow = NewUnitOfWork())
            {
                return Result.Success(_blockchainInfoRepositoryFactory.Create(uow).GetBlocks(limit, page));
            }
        }

        public Result<string> Search(string searchValue)
        {
            using (var uow = NewUnitOfWork())
            {
                if (long.TryParse(searchValue, out long blockNumber))
                {
                    if (NewRepository<Block>(uow).Exists(a => a.BlockNumber == blockNumber))
                        return Result.Success(SearchType.Block.ToString());
                }
                else
                {
                    if (NewRepository<Address>(uow).Exists(a => a.BlockchainAddress == searchValue))
                        return Result.Success(SearchType.Address.ToString());
                    if (NewRepository<Account>(uow).Exists(a => a.Hash == searchValue))
                        return Result.Success(SearchType.Account.ToString());
                    if (NewRepository<Asset>(uow).Exists(a => a.Hash == searchValue))
                        return Result.Success(SearchType.Asset.ToString());
                    if (NewRepository<Transaction>(uow).Exists(t => t.Hash == searchValue))
                        return Result.Success(SearchType.Transaction.ToString());
                    if (NewRepository<Equivocation>(uow).Exists(e => e.EquivocationProofHash == searchValue))
                        return Result.Success(SearchType.Equivocation.ToString());
                }

                return Result.Failure<string>("Not found.");
            }
        }
    }
}
