namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class AddressDto
    {
        public string BlockchainAddress { get; set; }
        public long Nonce { get; set; }
        public ChxBalanceDto Balance { get; set; }
    }
}
