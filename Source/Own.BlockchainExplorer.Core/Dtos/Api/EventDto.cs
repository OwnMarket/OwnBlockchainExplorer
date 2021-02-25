using System;
using System.Collections.Generic;
using Own.BlockchainExplorer.Core.Enums;
using Own.BlockchainExplorer.Core.Models;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class EventDto
    {
        public long? BlockNumber { get; set; }
        public DateTime? BlockTime { get; set; }
        public string TransactionHash { get; set; }
        public string EquivocationHash { get; set; }
        public string EventDetails { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Fee { get; set; }

        public static EventDto FromDomainModel(BlockchainEvent model)
        {
            var blockTimestamp = model.Block?.Timestamp;
            return new EventDto
            {
                BlockNumber = model.Block?.BlockNumber,
                BlockTime = blockTimestamp.HasValue
                    ? (DateTime?)DateTimeOffset.FromUnixTimeMilliseconds(blockTimestamp.Value).UtcDateTime
                    : null,
                TransactionHash = model.Tx?.Hash,
                EquivocationHash = model.Equivocation?.EquivocationProofHash,
                EventDetails = model.EventType +
                    (model.EventType == EventType.Action.ToString()
                    ? " " + model.TxAction.ActionNumber.ToString() + " - " + model.TxAction.ActionType
                    : ""),
                Amount = model.Amount,
                Fee = model.Fee
            };
        }

        public string GetCsvRow()
        {
            return string.Join(",", BlockNumber, BlockTime, TransactionHash, EventDetails, Amount, Fee);
        }
    }

    public class EventsSummaryDto
    {
        public List<EventDto> Events { get; set; }
        public int EventsCount { get; set; }
    }
}
