using System;
using System.Collections.Generic;
using System.Linq;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T> itemAction)
        {
            foreach (var item in enumerable)
            {
                itemAction(item);
            }

            return enumerable; // Enable method chaining
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            return enumerable.ContainsAny(elements.AsEnumerable());
        }

        public static bool ContainsAny<T>(this IEnumerable<T> enumerable, IEnumerable<T> elements)
        {
            if (enumerable.IsNullOrEmpty() || elements.IsNullOrEmpty())
                return false;

            return elements.Any(enumerable.Contains);
        }

        public static bool ContainsAll<T>(this IEnumerable<T> enumerable, params T[] elements)
        {
            return enumerable.ContainsAll(elements.AsEnumerable());
        }

        public static bool ContainsAll<T>(this IEnumerable<T> enumerable, IEnumerable<T> elements)
        {
            if (enumerable.IsNullOrEmpty() || elements.IsNullOrEmpty())
                return false;

            return elements.Any(enumerable.Contains);
        }

        public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return !enumerable.Any(predicate);
        }

        public static T? SingleOrNullable<T>(this IEnumerable<T> enumerable)
            where T : struct
        {
            return enumerable.IsNullOrEmpty() ? new T?() : enumerable.Single();
        }
    }
}
