using System;
using System.Collections.Generic;
using System.Text;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class FileDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public byte[] Content { get; set; }
    }
}
