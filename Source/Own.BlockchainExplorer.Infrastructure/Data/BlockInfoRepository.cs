using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Infrastructure.Data.EF;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class BlockInfoRepository : IBlockInfoRepository
    {
        private readonly OwnDb _db;
        public BlockInfoRepository(OwnDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IEnumerable<EquivocationInfoShortDto> GetEquivocationsInfo(long blockNumber, int page, int limit)
        {
            return
                _db.BlockchainEvents
                .Where(e => e.Block.BlockNumber == blockNumber && e.EventType == EventType.DepositTaken.ToString())
                .Include(e => e.Equivocation)
                .Include(e => e.Address)
                .Select(e => new EquivocationInfoShortDto
                {
                    EquivocationProofHash = e.Equivocation.EquivocationProofHash,
                    TakenDeposit = new DepositDto
                    {
                        BlockchainAddress = e.Address.BlockchainAddress,
                        Amount = -e.Amount.Value,
                        EquivocationProofHash = e.Equivocation.EquivocationProofHash
                    }
                })
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        }

        public IEnumerable<TxInfoShortDto> GetTransactionsInfo(long blockNumber, int page, int limit)
        {
            return
                _db.BlockchainEvents
                .Where(e => e.Block.BlockNumber == blockNumber && e.EventType == EventType.Action.ToString())
                .Include(e => e.Transaction)
                .Include(e => e.Address)
                .GroupBy(e => e.Transaction)
                .OrderBy(g => g.Key.TransactionId)
                .Select(g => new TxInfoShortDto
                {
                    Hash = g.Key.Hash,
                    NumberOfActions = g.Select(e => e.TxActionId).Distinct().Count(),
                    SenderAddress = g.First().Address.BlockchainAddress,
                    BlockNumber = blockNumber,
                    Status = g.Key.Status
                })
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        }
    }
}
