using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class BlockInfoDto
    {
        public long BlockNumber { get; set; }
        public string Hash { get; set; }
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
        public string Configuration { get; set; }
        public int? ConsensusRound { get; set; }
        public string Signatures { get; set; }

        public List<TxInfoShortDto> Transactions { get; set; }
        public List<EquivocationInfoShortDto> Equivocations { get; set; }
        public List<StakingRewardDto> StakingRewards { get; set; }
    }

    public class BlockInfoShortDto
    {
        public long BlockNumber { get; set; }
        public string Hash { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
