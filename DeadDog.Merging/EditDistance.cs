using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public static IEnumerable<Operation> GetOperations<T>(IImmutableList<T> from, IImmutableList<T> to)
        {
            return GetOperations(from, to, EqualityComparer<T>.Default.Equals);
        }
        public static IEnumerable<Operation> GetOperations<T>(IImmutableList<T> from, IImmutableList<T> to, Func<T, T, bool> equals)
        {
            int[,] ops = GetOperationsTable(from, to, equals, false);

            var operations = ImmutableList<Operation>.Empty;

            int si = 0, fi = 0;

            while (si != from.Count || fi != to.Count)
            {
                if (si == from.Count)
                {
                    operations = operations.Add(new Operation(si, OperationType.Insertion));
                    fi++;
                }
                else if (fi == to.Count)
                {
                    operations = operations.Add(new Operation(si, OperationType.Deletion));
                    si++;
                }
                else if (from[si].Equals(to[fi]))
                {
                    si++;
                    fi++;
                }
                else if (ops[si + 1, fi] <= ops[si, fi + 1])
                {
                    operations = operations.Add(new Operation(si, OperationType.Deletion));
                    si++;
                }
                else
                {
                    operations = operations.Add(new Operation(si, OperationType.Insertion));
                    fi++;
                }
            }
            return operations;
        }

        public static IEnumerable<IChange<T>> GetDifference<T>(IImmutableList<T> from, IImmutableList<T> to, bool allowReplace = true)
        {
            return GetDifference(from, to, EqualityComparer<T>.Default.Equals);
        }
        public static IEnumerable<IChange<T>> GetDifference<T>(IImmutableList<T> from, IImmutableList<T> to, Func<T, T, bool> equals)
        {
            var operations = GetOperations(from, to, equals)
                .OrderBy(x => x.Position)
                .ToImmutableList();

            var changes = new List<IChange<T>>();
            int pos_diff = 0;
            int offset_b = 0;

            while (pos_diff < operations.Count)
            {
                int length = 0;
                int pos_a_old = operations[pos_diff].Position;

                while (pos_diff < operations.Count && operations[pos_diff].Type == OperationType.Insertion)
                {
                    if (operations[pos_diff].Position != pos_a_old)
                        break;
                    length++;
                    pos_diff++;
                }
                if (length > 0)
                {
                    Range r = Range.FromStartLength(pos_a_old + offset_b, length);
                    int pos_a = pos_a_old;

                    var sub = to.GetRange(r);
                    changes.Add(new Insert<T>(sub, pos_a, r));
                    offset_b += length;
                }
                if (pos_diff >= operations.Count)
                    break;
                length = 0;
                pos_a_old = operations[pos_diff].Position;
                while (pos_diff < operations.Count && operations[pos_diff].Type == OperationType.Deletion)
                {
                    if (operations[pos_diff].Position != pos_a_old + length)
                        break;
                    length++;
                    pos_diff++;
                }
                if (length > 0)
                {
                    var r = Range.FromStartLength(pos_a_old, length);
                    int pos_b = pos_a_old + offset_b;

                    var sub = from.GetRange(r);
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
