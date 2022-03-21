using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class AssetBridgeService : IAssetBridgeService
    {
        private readonly IAssetBridgeRepository _assetBridgeRepository;
        private readonly IEthereumService _ethereumService;

        public AssetBridgeService(IAssetBridgeRepository assetBridgeRepository, IEthereumService ethereumService)
        {
            _assetBridgeRepository = assetBridgeRepository;
            _ethereumService = ethereumService;
        }
        
        public async Task<Result<List<BridgeTransferStatsInfoDto>>> GetBridgeTransferStats(string assetHash, int page, int limit)
        {
            var transfers = await _assetBridgeRepository.GetBridgeTransfers(assetHash, page, limit);
            var groupedTransfers = transfers.GroupBy(t => t.BlockchainCode);
            var transferStats = new List<BridgeTransferStatsInfoDto>();
            foreach (var group in groupedTransfers)
            {
                var contractAddress = (await _assetBridgeRepository.GetContractAddresses(assetHash, group.Key)).FirstOrDefault();
                var circulatingSupply = await GetCirculatingSupply(contractAddress, group.Key);
                if (circulatingSupply.Failed) continue;
                var transferStat = new BridgeTransferStatsInfoDto
                {
                    BlockchainCode = group.Key,
                    TransfersCount = group.Count(),
                    CirculatingSupply = circulatingSupply.Data,
                    ContractAddress = contractAddress
                };
                transferStats.Add(transferStat);
            }
            return Result.Success(transferStats);
        }

        private async Task<Result<decimal>> GetCirculatingSupply(string contractAddress, BlockchainCode blockchainCode)
        {
            try
            {
                return Result.Success(await _ethereumService.GetCirculatingSupply(contractAddress, blockchainCode));
            }
            catch (Exception)
            {
                return Result.Failure<decimal>("Oops. Something went wrong.");
            }
        }
    }
}