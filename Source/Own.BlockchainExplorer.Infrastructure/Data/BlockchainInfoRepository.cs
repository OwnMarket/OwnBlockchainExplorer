using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Infrastructure.Data.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;

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
    }
}
