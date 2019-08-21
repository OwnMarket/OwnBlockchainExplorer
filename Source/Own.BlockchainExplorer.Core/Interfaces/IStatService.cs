﻿using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IStatService
    {
        Result<IEnumerable<KeyValuePair<DateTime, int>>> GetTxPerDay(int numberOfDays);
        Result<IEnumerable<ValidatorStatsDto>> GetValidatorStats(int numberOfDays);
        Result<IEnumerable<KeyValuePair<string, decimal>>> GetTopAddresses(int page, int limit);
        Task<Result<ChxSupplyDto>> GetChxSupply();
    }
}
