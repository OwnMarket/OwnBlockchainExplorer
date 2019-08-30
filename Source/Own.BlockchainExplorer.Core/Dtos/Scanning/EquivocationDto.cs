using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class EquivocationDto
    {
        public string EquivocationProofHash { get; set; }
        public string ValidatorAddress { get; set; }
        public long BlockNumber { get; set; }
        public int ConsensusRound { get; set; }
        public string ConsensusStep { get; set; }
        public string EquivocationValue1 { get; set; }
        public string EquivocationValue2 { get; set; }
        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
        public decimal DepositTaken { get; set; }
        public List<DepositDto> DepositDistribution { get; set; }
        public long IncludedInBlockNumber { get; set; }
    }
}
