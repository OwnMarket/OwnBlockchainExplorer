using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Infrastructure.Data.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class BlockchainInfoRepository : IBlockchainInfoRepository
    {
        private readonly OwnDb _db;

        public BlockchainInfoRepository(OwnDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IEnumerable<TxInfoShortDto> GetTxs(int limit, int page)
        {
            /*return _db.Query<TxInfoShortDto>().FromSql("select t.transaction_id, t.hash, min(b.block_number), t.timestamp, count(distinct e.tx_action_id), min(addr.blockchain_address)" +
"from own.transaction t" +
"join own.blockchain_event e on t.transaction_id = e.transaction_id" +
"join own.block b on b.block_id = e.block_id" +
"join own.tx_action a on a.tx_action_id = e.tx_action_id" +
"join own.address addr on addr.address_id = e.address_id" +
"group by t.transaction_id" +
"order by t.timestamp desc" +
"limit 50").AsNoTracking().ToList();*/

            var query = (from t in _db.Transactions
                      join e in _db.BlockchainEvents on t.TransactionId equals e.TransactionId
                      join b in _db.Blocks on e.BlockId equals b.BlockId
                      join a in _db.TxActions on e.TxActionId equals a.TxActionId
                      join addr in _db.Addresses on e.AddressId equals addr.AddressId
                      group e by t into g
                      orderby g.Key.Timestamp descending
                      select new TxInfoShortDto
                      {
                          Hash = g.Key.Hash,
                          BlockNumber = g.First().Block.BlockNumber,
                          Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(g.Key.Timestamp),
                          SenderAddress = g.First().Address.BlockchainAddress,
                          NumberOfActions = g.Select(f => f.TxActionId).Distinct().Count(),
                          Status = g.Key.Status
                      }).Skip((page - 1) * limit).Take(limit).AsNoTracking();

            return query.ToList();
        }

        public IEnumerable<BlockInfoShortDto> GetBlocks(int limit, int page)
        {
            var query = (from b in _db.Blocks
                         orderby b.Timestamp descending
                         select new BlockInfoShortDto
                         {
                             Hash = b.Hash,
                             BlockNumber = b.BlockNumber,
                             Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(b.Timestamp)
                         }).Skip((page - 1) * limit).Take(limit).AsNoTracking();

            return query.ToList();
        }
    }
}
