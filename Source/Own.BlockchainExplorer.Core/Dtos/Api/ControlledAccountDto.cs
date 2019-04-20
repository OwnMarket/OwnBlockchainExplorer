using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ControlledAccountDto
    {
        public string Hash { get; set; }
        public bool IsActive { get; set; }
    }
}
