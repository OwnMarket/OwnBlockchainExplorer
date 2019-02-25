namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ChxBalanceInfoDto
    {
        public decimal TotalBalance { get; set; }
        public decimal DelegatedStakes { get; set; }
        public decimal ValidatorDeposit { get; set; }

        public decimal AvailableBalance => TotalBalance - DelegatedStakes - ValidatorDeposit;
    }
}
