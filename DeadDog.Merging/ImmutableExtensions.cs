using System.Collections.Immutable;
using System.Linq;

namespace DeadDog.Merging
{
    internal static class ImmutableExtensions
    {
        public static IImmutableList<T> GetRange<T>(this IImmutableList<T> list, int index, int count)
        {
            if (list is ImmutableList<T> actual)
                return actual.GetRange(index, count);
            else
                return list.Skip(index).Take(count).ToImmutableList();
        }
        public static IImmutableList<T> GetRange<T>(this IImmutableList<T> list, int index)
        {
            return GetRange(list, index, list.Count - index);
        }

        public static IImmutableList<T> GetRange<T>(this IImmutableList<T> list, Range range)
        {
            return GetRange(list, range.Start, range.Length);
        }
    }
}
