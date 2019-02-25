namespace Own.BlockchainExplorer.Core.Dtos.Scanning
{
    public class AddressDto
    {
        public string BlockchainAddress { get; set; }
        public long Nonce { get; set; }
        public decimal ChxBalance { get; set; }
    }
}
