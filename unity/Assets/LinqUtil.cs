using System.Collections.Generic;

namespace DefaultNamespace
{
    public static class LinqUtil
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }
    }
}