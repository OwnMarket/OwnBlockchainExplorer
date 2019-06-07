using Own.BlockchainExplorer.Core.Models;
using System;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class TxInfoDto
    {
        public string Hash { get; set; }
        public long BlockNumber { get; set; }
        public string SenderAddress { get; set; }
        public long Nonce { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public decimal ActionFee { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public short? FailedActionNumber { get; set; }

        public static TxInfoDto FromDomainModel(Transaction tx)
        {
            return new TxInfoDto
            {
                Hash = tx.Hash,
                Nonce = tx.Nonce,
                Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(tx.Timestamp),
                ExpirationTime = tx.ExpirationTime,
                ActionFee = tx.ActionFee,
                Status = tx.Status,
                ErrorMessage = tx.ErrorMessage,
                FailedActionNumber = tx.FailedActionNumber
            };
        }
    }

    public class TxInfoShortDto
    {
        public string Hash { get; set; }
        public long BlockNumber { get; set; }
        public string SenderAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public int NumberOfActions { get; set; }
        public string Status { get; set; }
    }
}
