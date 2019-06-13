using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TehGM.EinherjiBot.Extensions
{
    public static class EnumerableExtensions
    {
        public static string JoinAsSentence<T>(this IEnumerable<T> enumerable, string normalSeparator, string lastSeparator = " and ")
        {
            int count = enumerable?.Count() ?? default;
            if (count == default)
                return null;
            string lastValue = enumerable.Last().ToString();
            if (count == 1)
                return lastValue;
            return $"{string.Join(normalSeparator, enumerable.Take(count - 1))}{lastSeparator}{lastValue}";
        }
    }
}
