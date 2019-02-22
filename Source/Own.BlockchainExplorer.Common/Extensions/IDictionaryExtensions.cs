using System;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class IDictionaryExtensions
    {
        public static TValue? GetOrNull<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key)
            where TValue : struct
        {
            return dict.TryGetValue(key, out var value) ? value : (TValue?)null;
        }

        public static TValue GetOrElse<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue fallback)
        {
            return dict.TryGetValue(key, out var value) ? value : fallback;
        }

        public static TValue GetOrElse<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TKey, TValue> fn)
        {
            return dict.TryGetValue(key, out var value) ? value : fn(key);
        }
    }
}
