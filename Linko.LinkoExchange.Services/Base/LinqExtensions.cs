using System;
using System.Collections.Generic;
using System.Linq;

namespace Linko.LinkoExchange.Services.Base
{
    public static class LinqExtensions
    {
        public static IEnumerable<string> ExceptStringsCaseInsensitive
            (this IEnumerable<string> source, IEnumerable<string> value)
        {
            return source.Except(second:value, comparer:new StringCaseInsensitiveEqualityComparer());
        }

        public static IEnumerable<string> IntersectStringsCaseInsensitive
            (this IEnumerable<string> source, IEnumerable<string> value)
        {
            return source.Intersect(second:value, comparer:new StringCaseInsensitiveEqualityComparer());
        }

        public static bool CaseInsensitiveContains(this IEnumerable<string> source, string value)
        {
            return source.Contains(value:value, comparer:new StringCaseInsensitiveEqualityComparer());
        }

        public static Dictionary<string, TElement> ToCaseInsensitiveDictionary<TSource, TElement>
            (this IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dictionary = new Dictionary<string, TElement>(comparer:new StringCaseInsensitiveEqualityComparer());

            foreach (var sourceItem in source)
            {
                dictionary.Add(key:keySelector(arg:sourceItem), value:elementSelector(arg:sourceItem));
            }

            return dictionary;
        }
    }

    public class StringCaseInsensitiveEqualityComparer : IEqualityComparer<string>
    {
        #region Implementation of IEqualityComparer<in string>

        /// <inheritdoc />
        public bool Equals(string x, string y)
        {
            return string.Equals(a:x, b:y, comparisonType:StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public int GetHashCode(string value)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj:value);
        }

        #endregion
    }
}