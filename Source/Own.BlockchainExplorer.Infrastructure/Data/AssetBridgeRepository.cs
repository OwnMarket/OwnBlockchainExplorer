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
        public Task<List<BridgeTransferInfoDto>> GetBridgeTransfers(string assetHash) =>
            ReaderAction(
                $"select transfer_type_code, target_blockchain, amount from own.transfer where asset_hash = '{assetHash}' and transfer_status_code = 'Complete';",
                r => new BridgeTransferInfoDto
                {
                    TransferTypeCode = r[0].ToString().ToEnum<TransferType>(),
                    BlockchainCode = r[1].ToString().ToEnum<BlockchainCode>(),
                    Amount = decimal.Parse(r[2].ToString())
                }
            );
        
        public Task<List<string>> GetContractAddresses(string assetHash, BlockchainCode blockchainCode) =>
            ReaderAction(
                $"select token_address from own.asset where target_blockchain = '{blockchainCode.ToString()}' and asset_hash = '{assetHash}' and bridge_status_code = 'Bridged';",
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