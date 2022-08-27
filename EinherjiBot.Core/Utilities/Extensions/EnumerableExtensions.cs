namespace TehGM.EinherjiBot
{
    public static class EnumerableExtensions
    {
        public static string JoinAsSentence<T>(this IEnumerable<T> enumerable, string normalSeparator = ", ", string lastSeparator = " and ")
        {
            int count = enumerable?.Count() ?? default;
            if (count == default)
                return null;
            string lastValue = enumerable.Last().ToString();
            if (count == 1)
                return lastValue;
            return $"{string.Join(normalSeparator, enumerable.Take(count - 1))}{lastSeparator}{lastValue}";
        }

        public static bool Same<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer, bool ignoreElementsOrder = false)
        {
            if (first == null && second == null)
                return true;

            if (first == null && second != null)
                return false;
            if (first != null && second == null)
                return false;

            comparer ??= EqualityComparer<TSource>.Default;

            if (!ignoreElementsOrder)
                return first.SequenceEqual(second, comparer);

            return first.Count() == second.Count() && first.All(i => second.Contains(i, comparer));
        }

        public static bool Same<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, bool ignoreElementsOrder = false)
            => Same(first, second, null, ignoreElementsOrder);

        public static bool IsSubsetOf<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first?.Any() != true)
                return true;

            if (second == null)
                return false;

            comparer = EqualityComparer<TSource>.Default;
            return !first.Except(second, comparer).Any();
        }

        public static bool IsSubsetOf<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
            => IsSubsetOf(first, second, null);
    }
}
