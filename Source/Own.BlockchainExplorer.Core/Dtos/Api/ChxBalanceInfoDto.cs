namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ChxBalanceInfoDto
    {
        public decimal AvailableBalance { get; set; }
        public decimal DelegatedStakes { get; set; }
        public decimal ValidatorDeposit { get; set; }

        public decimal TotalBalance => AvailableBalance + DelegatedStakes + ValidatorDeposit;
    }
}
