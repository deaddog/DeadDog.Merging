using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public static class EditDistance
    {
        public static int GetDistance<T>(T[] a, T[] b, bool allowReplace = true) where T : IEquatable<T>
        {
            return getOperationsTable(a, b, allowReplace)[0, 0];
        }
        public static double GetNormalizedDistance<T>(T[] a, T[] b, bool allowReplace = true) where T : IEquatable<T>
        {
            double distance = GetDistance(a, b, allowReplace);
            if (allowReplace)
                return distance / (double)Math.Max(a.Length, b.Length);
            else
                return distance / (double)(a.Length + b.Length);
        }

        public static int GetDistance<T>(T[] a, T[] b, Func<T, T, bool> equals, bool allowReplace = true)
        {
            return getOperationsTable(a, b, equals, allowReplace)[0, 0];
        }
        public static double GetNormalizedDistance<T>(T[] a, T[] b, Func<T, T, bool> equals, bool allowReplace = true)
        {
            double distance = GetDistance(a, b, equals, allowReplace);
            if (allowReplace)
                return distance / (double)Math.Max(a.Length, b.Length);
            else
                return distance / (double)(a.Length + b.Length);
        }

        public static Tuple<int, ChangeType>[] GetOperations<T>(T[] a, T[] b) where T : IEquatable<T>
        {
            int[,] d3 = getOperationsTable(a, b, false);
            List<Tuple<int, ChangeType>> changes = new List<Tuple<int, ChangeType>>();
            int si = 0, fi = 0;
            int ls = a.Length, lf = b.Length;

            while (si != a.Length || fi != b.Length)
            {
                if (si == a.Length)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Insertion));
                    fi++;
                }
                else if (fi == b.Length)
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
        public static Tuple<int, ChangeType>[] GetOperations<T>(T[] a, T[] b, Func<T, T, bool> equals)
        {
            int[,] d3 = getOperationsTable(a, b, equals, false);
            List<Tuple<int, ChangeType>> changes = new List<Tuple<int, ChangeType>>();
            int si = 0, fi = 0;
            int ls = a.Length, lf = b.Length;

            while (si != a.Length || fi != b.Length)
            {
                if (si == a.Length)
                {
                    changes.Add(Tuple.Create(si, ChangeType.Insertion));
                    fi++;
                }
                else if (fi == b.Length)
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

        private static int[,] getOperationsTable<T>(T[] a, T[] b, bool allowReplace) where T : IEquatable<T>
        {
            int[,] operations = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                operations[i, b.Length] = a.Length - i;
            for (int i = 0; i <= b.Length; i++)
                operations[a.Length, i] = b.Length - i;

            for (int j = b.Length - 1; j >= 0; j--)
                for (int i = a.Length - 1; i >= 0; i--)
                    if (a[i].Equals(b[j]))
                        operations[i, j] = operations[i + 1, j + 1];
                    else if (allowReplace)
                        operations[i, j] = Math.Min(Math.Min(operations[i + 1, j], operations[i, j + 1]), operations[i + 1, j + 1]) + 1;
                    else
                        operations[i, j] = Math.Min(operations[i + 1, j], operations[i, j + 1]) + 1;

            return operations;
        }
        private static int[,] getOperationsTable<T>(T[] a, T[] b, Func<T, T, bool> equals, bool allowReplace)
        {
            int[,] operations = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                operations[i, b.Length] = a.Length - i;
            for (int i = 0; i <= b.Length; i++)
                operations[a.Length, i] = b.Length - i;

            for (int j = b.Length - 1; j >= 0; j--)
                for (int i = a.Length - 1; i >= 0; i--)
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
