using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class BlockDto
    {
        public long Number { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public long ConfigurationBlockNumber { get; set; }
        public long Timestamp { get; set; }
        public string ProposerAddress { get; set; }
        public string TxSetRoot { get; set; }
        public string TxResultSetRoot { get; set; }
        public string EquivocationProofsRoot { get; set; }
        public string EquivocationProofResultsRoot { get; set; }
        public string StateRoot { get; set; }
        public string StakingRewardsRoot { get; set; }
        public string ConfigurationRoot { get; set; }
        public ConfigurationDto Configuration { get; set; }
        public int? ConsensusRound { get; set; }

        public List<string> TxSet { get; set; }
        public List<string> EquivocationProofs { get; set; }
        public List<StakingRewardDto> StakingRewards { get; set; }
        public List<string> Signatures { get; set; }
    }
}
