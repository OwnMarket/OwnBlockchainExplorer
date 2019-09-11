using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Infrastructure.Data.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class BlockchainInfoRepository : IBlockchainInfoRepository
    {
        private readonly OwnDb _db;

        public BlockchainInfoRepository(OwnDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IEnumerable<Transaction> GetTxs(int limit, int page)
        {
            return
                _db.Transactions
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        }

        public IEnumerable<BlockInfoShortDto> GetBlocks(int limit, int page)
        {
            var query =
                _db.Blocks
                .OrderByDescending(b => b.Timestamp)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(b => new BlockInfoShortDto
                    {
                        Hash = b.Hash,
                        BlockNumber = b.BlockNumber,
                        Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(b.Timestamp)
                    })
                .AsNoTracking();

            return query.ToList();
        }

        public Dictionary<long, int> GetValidatorProposedBlockCount(long minTimestamp)
        {
            return
                _db.Blocks
                    .Where(b => b.Timestamp > minTimestamp)
                    .Select(b => new { b.ValidatorId, b.BlockId })
                    .GroupBy(b => b.ValidatorId)
                    .Select(g => new KeyValuePair<long, int>(g.Key, g.Select(s => s.BlockId).Count()))
                    .ToDictionary(g => g.Key, g => g.Value);
        }

        public Dictionary<long, int> GetValidatorProposedTxCount(long minTimestamp)
        {
            return
                _db.Validators
                    .Where(v => !v.IsDeleted)
                    .Join(
                        _db.Blocks.Where(b => b.Timestamp > minTimestamp),
                        v => v.ValidatorId,
                        b => b.ValidatorId,
                        (v, b) => new {v.ValidatorId, b.BlockId})
                    .Join(
                        _db.BlockchainEvents.Where(ev => ev.TransactionId.HasValue),
                        vb => vb.BlockId,
                        e => e.BlockId,
                        (vb, e) => new { vb.ValidatorId, vb.BlockId, BlockchainEvent = e})
                    .GroupBy(s => s.ValidatorId)
                    .Select(g => new KeyValuePair<long, int>(g.Key, g.Select(s => s.BlockchainEvent).Count()))
                    .ToDictionary(g => g.Key, g => g.Value);
        }

        public Dictionary<string, decimal> GetReceivedStakes()
        {
            return
                _db.BlockchainEvents
                    .Where(
                        e => e.EventType == EventType.Action.ToString()
                        && e.TxAction.ActionType == ActionType.DelegateStake.ToString()
                        && e.Fee == null
                        && e.Transaction.Status == TxStatus.Success.ToString()
                        && e.Amount.HasValue
                        && e.Amount.Value > 0)
                    .Select(e => new { e.Address.BlockchainAddress, e.Amount.Value })
                    .GroupBy(s => s.BlockchainAddress)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(s => s.Value)))
                    .ToDictionary(g => g.Key, g => g.Value);
        }

        public Dictionary<string, decimal> GetValidatorRewards(long minTimestamp)
        {
            return
                _db.BlockchainEvents
                    .Where(
                        e => e.EventType == EventType.ValidatorReward.ToString()
                        && e.Block.Timestamp > minTimestamp
                        && e.Amount.HasValue
                        && e.Amount.Value > 0)
                    .Select(e => new { e.Address.BlockchainAddress, e.Amount.Value })
                    .GroupBy(s => s.BlockchainAddress)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(s => s.Value)))
                    .ToDictionary(g => g.Key, g => g.Value);
        }

        public Dictionary<long, decimal> GetValidatorStakingRewards(long minTimestamp)
        {
            return
                _db.Validators
                    .Where(v => !v.IsDeleted)
                    .Join(
                        _db.Blocks.Where(b => b.Timestamp > minTimestamp),
                        v => v.ValidatorId,
                        b => b.ValidatorId,
                        (v, b) => new { v.ValidatorId, b.BlockId })
                    .Join(
                        _db.BlockchainEvents.Where(
                            e => e.EventType == EventType.StakingReward.ToString()
                            && e.Block.Timestamp > minTimestamp
                            && e.Amount.HasValue
                            && e.Amount.Value > 0),
                        vb => vb.BlockId,
                        e => e.BlockId,
                        (vb, e) => new { vb.ValidatorId, vb.BlockId, e.Amount.Value })
                    .GroupBy(s => s.ValidatorId)
                    .Select(g => new KeyValuePair<long, decimal>(g.Key, g.Sum(s => s.Value)))
                    .ToDictionary(g => g.Key, g => g.Value);
        }
    }
}
