using System;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan AddMinutes(this TimeSpan value, decimal minutes) =>
            value.Add(TimeSpan.FromMinutes(Convert.ToDouble(minutes)));
    }
}
