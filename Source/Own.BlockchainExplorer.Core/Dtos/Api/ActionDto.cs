using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ActionDto
    {
        public int ActionNumber { get; set; }
        public string ActionType { get; set; }
        public string ActionData { get; set; }
        public string TxHash { get; set; }
    }
}
