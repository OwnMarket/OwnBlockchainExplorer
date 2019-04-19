////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Models
{
    public partial class Action
    {
        public long ActionId { get; set; }
        public int ActionNumber { get; set; }
        public string ActionType { get; set; }
        public string ActionData { get; set; }
        
        public virtual ICollection<Event> EventsByActionId { get; set; }

        public Action()
        {
            this.EventsByActionId = new HashSet<Event>();
        }
    }
}