using System;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common.Extensions;
using Own.BlockchainExplorer.Core.Models;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class BlockInfoDto
    {
        public long BlockNumber { get; set; }
        public string Hash { get; set; }
        public long? PreviousBlockNumber { get; set; }
        public string PreviousBlockHash { get; set; }
        public long ConfigurationBlockNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string ValidatorAddress { get; set; }
        public string TxSetRoot { get; set; }
        public string TxResultSetRoot { get; set; }
        public string EquivocationProofsRoot { get; set; }
        public string EquivocationProofResultsRoot { get; set; }
        public string StateRoot { get; set; }
        public string StakingRewardsRoot { get; set; }
        public string ConfigurationRoot { get; set; }
        public BlockConfigurationDto Configuration { get; set; }
        public int? ConsensusRound { get; set; }
        public string Signatures { get; set; }

        public static BlockInfoDto FromDomainModel(Block block)
        {
            return new BlockInfoDto
            {
                BlockNumber = block.BlockNumber,
                Hash = block.Hash,
                PreviousBlockHash = block.PreviousBlockHash,
                PreviousBlockNumber = block.PreviousBlock?.BlockNumber,
                ConfigurationBlockNumber = block.ConfigurationBlockNumber,
                Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(block.Timestamp),
                ValidatorAddress = block.Validator.BlockchainAddress,
                TxSetRoot = block.TxSetRoot,
                TxResultSetRoot = block.TxResultSetRoot,
                EquivocationProofsRoot = block.EquivocationProofsRoot,
                EquivocationProofResultsRoot = block.EquivocationProofResultsRoot,
                StateRoot = block.StateRoot,
                StakingRewardsRoot = block.StakingRewardsRoot,
                ConfigurationRoot = block.ConfigurationRoot,
                Configuration = GetBlockConfiguration(block.Configuration),
                ConsensusRound = block.ConsensusRound,
                Signatures = block.Signatures
            };
        }

        private static BlockConfigurationDto GetBlockConfiguration(string configuration)
        {
            if (!configuration.IsNullOrEmpty())
                return JsonConvert.DeserializeObject<BlockConfigurationDto>(configuration);
            return null;
        }
    }

    public class BlockInfoShortDto
    {
        public long BlockNumber { get; set; }
        public string Hash { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
