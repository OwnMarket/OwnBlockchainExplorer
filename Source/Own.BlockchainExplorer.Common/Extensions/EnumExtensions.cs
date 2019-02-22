using System;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, bool ignoreCase = false)
            where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("Provided type is not an enum.");

            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static T ToEnum<T>(this string value, T defaultValue, bool ignoreCase = false)
            where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("Provided type is not an enum.");

            return Enum.TryParse<T>(value, ignoreCase, out var enumValue) ? enumValue : defaultValue;
        }
    }
}
