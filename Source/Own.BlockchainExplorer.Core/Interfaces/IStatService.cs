using Own.BlockchainExplorer.Common.Framework;
using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IStatService
    {
        Result<IEnumerable<KeyValuePair<DateTime, int>>> GetTxPerDay(int numberOfDays);
    }
}
