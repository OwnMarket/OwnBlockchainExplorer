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
        
        public async Task<Result<List<BridgeTransferStatsInfoDto>>> GetBridgeTransferStats(string assetHash)
        {
            var transfers = await _assetBridgeRepository.GetAllBridgeTransfers(assetHash);
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
        
        public async Task<Result<List<BridgeTransferInfoDto>>> GetBridgeTransfers(string assetHash, int page, int limit)
        {
            try
            {
                return Result.Success(await _assetBridgeRepository.GetBridgeTransfers(assetHash, page, limit));
            }
            catch (Exception)
            {
                return Result.Failure<List<BridgeTransferInfoDto>>("Oops. Something went wrong.");
            }
            
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