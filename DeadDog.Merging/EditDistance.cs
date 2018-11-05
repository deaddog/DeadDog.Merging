using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public static class EditDistance
    {
        public static int GetDistance<T>(IImmutableList<T> a, IImmutableList<T> b, bool allowReplace = true) where T : IEquatable<T>
        {
            return getOperationsTable(a, b, allowReplace)[0, 0];
        }
        public static double GetNormalizedDistance<T>(IImmutableList<T> a, IImmutableList<T> b, bool allowReplace = true) where T : IEquatable<T>
        {
            double distance = GetDistance(a, b, allowReplace);
            if (allowReplace)
                return distance / (double)Math.Max(a.Count, b.Count);
            else
                return distance / (double)(a.Count + b.Count);
        }

        public static int GetDistance<T>(IImmutableList<T> a, IImmutableList<T> b, Func<T, T, bool> equals, bool allowReplace = true)
        {
            return getOperationsTable(a, b, equals, allowReplace)[0, 0];
        }
        public static double GetNormalizedDistance<T>(IImmutableList<T> a, IImmutableList<T> b, Func<T, T, bool> equals, bool allowReplace = true)
        {
            double distance = GetDistance(a, b, equals, allowReplace);
            if (allowReplace)
                return distance / (double)Math.Max(a.Count, b.Count);
            else
                return distance / (double)(a.Count + b.Count);
        }

        public static Tuple<int, ChangeType>[] GetOperations<T>(IImmutableList<T> a, IImmutableList<T> b) where T : IEquatable<T>
        {
            int[,] d3 = getOperationsTable(a, b, false);
            List<Tuple<int, ChangeType>> changes = new List<Tuple<int, ChangeType>>();
            int si = 0, fi = 0;
            int ls = a.Count, lf = b.Count;

            while (si != a.Count || fi != b.Count)
            {
                if (si == a.Count)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Insertion));
                    fi++;
                }
                else if (fi == b.Count)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Deletion));
                    si++;
                }
                else if (a[si].Equals(b[fi]))
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
        public static Tuple<int, ChangeType>[] GetOperations<T>(IImmutableList<T> a, IImmutableList<T> b, Func<T, T, bool> equals)
        {
            int[,] d3 = getOperationsTable(a, b, equals, false);
            List<Tuple<int, ChangeType>> changes = new List<Tuple<int, ChangeType>>();
            int si = 0, fi = 0;
            int ls = a.Count, lf = b.Count;

            while (si != a.Count || fi != b.Count)
            {
                if (si == a.Count)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Insertion));
                    fi++;
                }
                else if (fi == b.Count)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Deletion));
                    si++;
                }
                else if (a[si].Equals(b[fi]))
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

        private static int[,] getOperationsTable<T>(IImmutableList<T> a, IImmutableList<T> b, bool allowReplace) where T : IEquatable<T>
        {
            int[,] operations = new int[a.Count + 1, b.Count + 1];

            for (int i = 0; i <= a.Count; i++)
                operations[i, b.Count] = a.Count - i;
            for (int i = 0; i <= b.Count; i++)
                operations[a.Count, i] = b.Count - i;

            for (int j = b.Count - 1; j >= 0; j--)
                for (int i = a.Count - 1; i >= 0; i--)
                    if (a[i].Equals(b[j]))
                        operations[i, j] = operations[i + 1, j + 1];
                    else if (allowReplace)
                        operations[i, j] = Math.Min(Math.Min(operations[i + 1, j], operations[i, j + 1]), operations[i + 1, j + 1]) + 1;
                    else
                        operations[i, j] = Math.Min(operations[i + 1, j], operations[i, j + 1]) + 1;

            return operations;
        }
        private static int[,] getOperationsTable<T>(IImmutableList<T> a, IImmutableList<T> b, Func<T, T, bool> equals, bool allowReplace)
        {
            int[,] operations = new int[a.Count + 1, b.Count + 1];

            for (int i = 0; i <= a.Count; i++)
                operations[i, b.Count] = a.Count - i;
            for (int i = 0; i <= b.Count; i++)
                operations[a.Count, i] = b.Count - i;

            for (int j = b.Count - 1; j >= 0; j--)
                for (int i = a.Count - 1; i >= 0; i--)
                    if (equals(a[i], b[j]))
                        operations[i, j] = operations[i + 1, j + 1];
                    else if (allowReplace)
                        operations[i, j] = Math.Min(Math.Min(operations[i + 1, j], operations[i, j + 1]), operations[i + 1, j + 1]) + 1;
                    else
                        operations[i, j] = Math.Min(operations[i + 1, j], operations[i, j + 1]) + 1;

            return operations;
        }
    }
}
