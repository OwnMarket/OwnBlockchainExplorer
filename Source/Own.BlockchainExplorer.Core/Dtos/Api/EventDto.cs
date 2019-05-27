using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class EventDto
    {
        public long? BlockNumber { get; set; }
        public string TransactionHash { get; set; }
        public string EquivocationHash { get; set; }
        public string EventDetails { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Fee { get; set; }

        public static EventDto FromDomainModel(BlockchainEvent model)
        {
            return new EventDto
            {
                BlockNumber = model.Block?.BlockNumber,
                TransactionHash = model.Transaction?.Hash,
                EquivocationHash = model.Equivocation?.EquivocationProofHash,
                EventDetails = model.EventType +
                    (model.EventType == EventType.Action.ToString()
                    ? " " + model.TxAction.ActionNumber.ToString() + " - " + model.TxAction.ActionType
                    : ""),
                Amount = model.Amount,
                Fee = model.Fee
            };
        }
    }
}
