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

        public static IEnumerable<IChange<T>> GetDifference<T>(IImmutableList<T> from, IImmutableList<T> to, bool allowReplace = true)
        {
            return GetDifference(from, to, EqualityComparer<T>.Default.Equals);
        }
        public static IEnumerable<IChange<T>> GetDifference<T>(IImmutableList<T> a, IImmutableList<T> b, Func<T, T, bool> equals)
        {
            var diff = GetOperations(a, b, equals);
            Array.Sort(diff, (x, y) => x.Item1.CompareTo(y.Item1));

            var changes = new List<IChange<T>>();
            int pos_diff = 0;
            int offset_b = 0;

            while (pos_diff < diff.Length)
            {
                int length = 0;
                int pos_a_old = diff[pos_diff].Item1;

                while (pos_diff < diff.Length && diff[pos_diff].Item2 == ChangeType.Insertion)
                {
                    if (diff[pos_diff].Item1 != pos_a_old)
                        break;
                    length++;
                    pos_diff++;
                }
                if (length > 0)
                {
                    Range r = Range.FromStartLength(pos_a_old + offset_b, length);
                    int pos_a = pos_a_old;

                    var sub = b.GetRange(r);
                    changes.Add(new Insert<T>(sub, pos_a, r));
                    offset_b += length;
                }
                if (pos_diff >= diff.Length)
                    break;
                length = 0;
                pos_a_old = diff[pos_diff].Item1;
                while (pos_diff < diff.Length && diff[pos_diff].Item2 == ChangeType.Deletion)
                {
                    if (diff[pos_diff].Item1 != pos_a_old + length)
                        break;
                    length++;
                    pos_diff++;
                }
                if (length > 0)
                {
                    Range r = Range.FromStartLength(pos_a_old, length);
                    int pos_b = pos_a_old + offset_b;

                    var sub = a.GetRange(r);
                    changes.Add(new Delete<T>(sub, r, pos_b));
                    offset_b -= length;
                }
            }

            return changes;
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
