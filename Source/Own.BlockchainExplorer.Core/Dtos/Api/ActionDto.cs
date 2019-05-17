using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ActionDto
    {
        public int ActionNumber { get; set; }
        public string ActionType { get; set; }
        public string ActionData { get; set; }
        public string TxHash { get; set; }

        public static ActionDto FromDomainModel(TxAction action)
        {
            return new ActionDto
            {
                ActionNumber = action.ActionNumber,
                ActionType = action.ActionType,
                ActionData = action.ActionData
            };
        }
    }

    public class ActionDtoEqualityComparer : IEqualityComparer<ActionDto>
    {
        public bool Equals(ActionDto x, ActionDto y)
        {
            return x.TxHash == y.TxHash && x.ActionNumber == y.ActionNumber;
        }
        public int GetHashCode(ActionDto obj)
        {
            return (obj.TxHash + obj.ActionNumber).GetHashCode();
        }
    }
}
