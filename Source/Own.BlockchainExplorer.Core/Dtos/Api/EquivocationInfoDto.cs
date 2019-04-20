using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class EquivocationInfoDto
    {
        public string EquivocationProofHash { get; set; }
        public long BlockNumber { get; set; }
        public int ConsensusRound { get; set; }
        public int ConsensusStep { get; set; }
        public string EquivocationValue1 { get; set; }
        public string EquivocationValue2 { get; set; }
        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
        public long IncludedInBlockNumber { get; set; }

        public DepositDto TakenDeposit { get; set; }
        public List<DepositDto> GivenDeposits { get; set; }
    }

    public class EquivocationInfoShortDto
    {
        public string EquivocationProofHash { get; set; }
        public DepositDto TakenDeposit { get; set; }
    }
}
