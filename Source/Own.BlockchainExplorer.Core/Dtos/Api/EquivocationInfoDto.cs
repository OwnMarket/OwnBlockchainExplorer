using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class EquivocationInfoDto
    {
        public string EquivocationProofHash { get; set; }
        public long BlockNumber { get; set; }
        public int ConsensusRound { get; set; }
        public string ConsensusStep { get; set; }
        public string EquivocationValue1 { get; set; }
        public string EquivocationValue2 { get; set; }
        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
        public long IncludedInBlockNumber { get; set; }

        public DepositDto TakenDeposit { get; set; }
        public List<DepositDto> GivenDeposits { get; set; }

        public static EquivocationInfoDto FromDomainModel(Equivocation equivocation)
        {
            return new EquivocationInfoDto
            {
                EquivocationProofHash = equivocation.EquivocationProofHash,
                BlockNumber = equivocation.BlockNumber,
                ConsensusRound = equivocation.ConsensusRound,
                ConsensusStep = equivocation.ConsensusStep,
                EquivocationValue1 = equivocation.EquivocationValue1,
                EquivocationValue2 = equivocation.EquivocationValue2,
                Signature1 = equivocation.Signature1,
                Signature2 = equivocation.Signature2
            };
        }
    }

    public class EquivocationInfoShortDto
    {
        public string EquivocationProofHash { get; set; }
        public DepositDto TakenDeposit { get; set; }
    }
}
