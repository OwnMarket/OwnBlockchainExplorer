namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class SubmitVoteWeightData
    {
        public string AccountHash { get; set; }
        public string AssetHash { get; set; }
        public string ResolutionHash { get; set; }
        public decimal VoteWeight { get; set; }
    }
}
