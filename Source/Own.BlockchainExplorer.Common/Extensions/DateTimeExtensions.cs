using System;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime EndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);
        public static string IsoDateString(this DateTime dateTime) => dateTime.ToString("yyyy-MM-dd");
    }
}
