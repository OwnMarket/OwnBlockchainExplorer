namespace Own.BlockchainExplorer.Core.Dtos.ActionData
{
    public class SubmitVoteData
    {
        public string AccountHash { get; set; }
        public string AssetHash { get; set; }
        public string ResolutionHash { get; set; }
        public string VoteHash { get; set; }
    }
}
