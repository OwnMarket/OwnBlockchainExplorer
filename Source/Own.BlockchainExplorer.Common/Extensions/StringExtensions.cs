using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool ContainsAny(this string str, params string[] strings)
        {
            return str.ContainsAny(strings.AsEnumerable());
        }

        public static bool ContainsAny(this string str, IEnumerable<string> strings)
        {
            if (str.IsNullOrEmpty() || strings == null)
                return false;

            return strings.Any(str.Contains);
        }

        public static bool ContainsAll(this string str, params string[] strings)
        {
            return str.ContainsAll(strings.AsEnumerable());
        }

        public static bool ContainsAll(this string str, IEnumerable<string> strings)
        {
            if (str.IsNullOrEmpty() || strings == null)
                return false;

            return strings.All(str.Contains);
        }

        /// <summary>
        /// Calls String.Format method using passed IFormatProvider (CultureInfo).
        /// </summary>
        public static string F(this string str, IFormatProvider provider, params object[] args)
        {
            return string.Format(provider, str, args);
        }

        /// <summary>
        /// Calls String.Format method using CultureInfo.InvariantCulture.
        /// </summary>
        public static string F(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// Calls String.Format method using CultureInfo.InvariantCulture.
        /// </summary>
        public static string FormatInvariant(this string str, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, str, args);
        }
    }
}
