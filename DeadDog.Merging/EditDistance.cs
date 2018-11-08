using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public static class EditDistance
    {
        public static int GetDistance<T>(IImmutableList<T> from, IImmutableList<T> to, bool allowReplace = true)
        {
            return GetDistance(from, to, EqualityComparer<T>.Default.Equals, allowReplace);
        }
        public static int GetDistance<T>(IImmutableList<T> from, IImmutableList<T> to, Func<T, T, bool> equals, bool allowReplace = true)
        {
            return GetOperationsTable(from, to, equals, allowReplace)[0, 0];
        }

        public static double GetNormalizedDistance<T>(IImmutableList<T> from, IImmutableList<T> to, bool allowReplace = true)
        {
            return GetNormalizedDistance(from, to, EqualityComparer<T>.Default.Equals, allowReplace);
        }
        public static double GetNormalizedDistance<T>(IImmutableList<T> from, IImmutableList<T> to, Func<T, T, bool> equals, bool allowReplace = true)
        {
            double distance = GetDistance(from, to, equals, allowReplace);
            if (allowReplace)
                return distance / (double)Math.Max(from.Count, to.Count);
            else
                return distance / (double)(from.Count + to.Count);
        }

        public static Tuple<int, ChangeType>[] GetOperations<T>(IImmutableList<T> from, IImmutableList<T> to)
        {
            return GetOperations(from, to, EqualityComparer<T>.Default.Equals);
        }
        public static Tuple<int, ChangeType>[] GetOperations<T>(IImmutableList<T> from, IImmutableList<T> to, Func<T, T, bool> equals)
        {
            int[,] d3 = GetOperationsTable(from, to, equals, false);
            var changes = new List<Tuple<int, ChangeType>>();
            int si = 0, fi = 0;
            int ls = from.Count, lf = to.Count;

            while (si != from.Count || fi != to.Count)
            {
                if (si == from.Count)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Insertion));
                    fi++;
                }
                else if (fi == to.Count)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Deletion));
                    si++;
                }
                else if (from[si].Equals(to[fi]))
                { si++; fi++; }
                else if (d3[si + 1, fi] <= d3[si, fi + 1])
                {
                    changes.Add(Tuple.Create(si, ChangeType.Deletion));
                    si++;
                }
                else
                {
                    changes.Add(Tuple.Create(si, ChangeType.Insertion));
                    fi++;
                }
            }
            return changes.ToArray();
        }

        private static int[,] GetOperationsTable<T>(IImmutableList<T> from, IImmutableList<T> to, Func<T, T, bool> equals, bool allowReplace)
        {
            int[,] operations = new int[from.Count + 1, to.Count + 1];

            for (int i = 0; i <= from.Count; i++)
                operations[i, to.Count] = from.Count - i;
            for (int i = 0; i <= to.Count; i++)
                operations[from.Count, i] = to.Count - i;

            for (int j = to.Count - 1; j >= 0; j--)
                for (int i = from.Count - 1; i >= 0; i--)
                    if (equals(from[i], to[j]))
                        operations[i, j] = operations[i + 1, j + 1];
                    else if (allowReplace)
                        operations[i, j] = Math.Min(Math.Min(operations[i + 1, j], operations[i, j + 1]), operations[i + 1, j + 1]) + 1;
                    else
                        operations[i, j] = Math.Min(operations[i + 1, j], operations[i, j + 1]) + 1;

            return operations;
        }
    }
}
