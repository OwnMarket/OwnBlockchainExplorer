namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class AddressInfoDto
    {
        public string BlockchainAddress { get; set; }
        public long Nonce { get; set; }
        public ChxBalanceInfoDto ChxBalanceInfo { get; set; }
    }
}
