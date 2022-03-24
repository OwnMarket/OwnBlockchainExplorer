using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Core;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class AssetBridgeRepository : IAssetBridgeRepository
    {
        public Task<List<BridgeTransferInfoDto>> GetAllBridgeTransfers(string assetHash) =>
            ReaderAction(
                $"select transfer_type_code, target_blockchain, amount from transfer where asset_hash = '{assetHash}' and transfer_status_code = 'Complete';",
                r => new BridgeTransferInfoDto
                {
                    TransferTypeCode = r[0].ToString().ToEnum<TransferType>(),
                    BlockchainCode = r[1].ToString().ToEnum<BlockchainCode>(),
                    Amount = decimal.Parse(r[2].ToString())
                }
            );
        
        public Task<List<BridgeTransferInfoDto>> GetBridgeTransfers(string assetHash, int page, int limit) =>
            ReaderAction(
                $"select transfer_type_code, target_blockchain, amount, block_time, original_tx_hash, swap_tx_hash from transfer where asset_hash = '{assetHash}' and transfer_status_code = 'Complete' order by block_time desc offset {(page-1) * limit} limit {limit};",
                r => new BridgeTransferInfoDto
                {
                    TransferTypeCode = r[0].ToString().ToEnum<TransferType>(),
                    BlockchainCode = r[1].ToString().ToEnum<BlockchainCode>(),
                    Amount = decimal.Parse(r[2].ToString()),
                    BlockTime = DateTime.Parse(r[3].ToString()),
                    OriginalTxHash = r[4].ToString(),
                    SwapTxHash = r[5].ToString()
                }
            );
        
        public Task<List<string>> GetContractAddresses(string assetHash, BlockchainCode blockchainCode) =>
            ReaderAction(
                $"select token_address from asset where target_blockchain = '{blockchainCode.ToString()}' and asset_hash = '{assetHash}' and bridge_status_code = 'Bridged';",
                r => r[0].ToString()
            );
        
        private static Task<List<T>> ReaderAction<T>(string query, Func<DbDataReader, T> action) =>
            Action(query, async c =>
            {
                using (var reader = await c.ExecuteReaderAsync())
                {
                    var list = new List<T>();
                    while (await reader.ReadAsync())
                    {
                        var resp = action(reader);
                        list.Add(resp);
                    }
                    return list;
                }
            });
        
        private static async Task<T> Action<T>(string query, Func<NpgsqlCommand, Task<T>> action)
        {
            using (var connection = new NpgsqlConnection(Config.BridgeDB))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    return await action(command);
                }
            }
        }
    }
}