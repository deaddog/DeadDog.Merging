using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class OptimalDiff<T> where T : IEquatable<T>
    {
        private T[] a, b;

        private OptimalDiff(T[] a, T[] b)
        {
            this.a = a;
            this.b = b;
        }

        public static List<IChange<T[]>> Diff(IEnumerable<T> a, IEnumerable<T> b)
        {
            var od = new OptimalDiff<T>(a.ToArray(), b.ToArray());
            return od.str_diff();
        }

        public static List<IChange<T2>> Diff<T2>(T2 a, T2 b, Func<T2, IEnumerable<T>> split, Func<T[], T2> join)
        {
            var temp = Diff(split(a), split(b));
            List<IChange<T2>> n = new List<IChange<T2>>();
            for (int i = 0; i < temp.Count; i++)
                n.Add(temp[i].Clone(join(temp[i].Value)));
            return n;
        }

        private List<Tuple<int, ChangeType>> min_diff()
        {
            int[,] d3 = get_operations();
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
            return changes;
        }

        private int[,] get_operations()
        {
            int[,] operations = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                operations[i, b.Length] = a.Length - i;
            for (int i = 0; i <= b.Length; i++)
                operations[a.Length, i] = b.Length - i;

            for (int j = b.Length - 1; j >= 0; j--)
                for (int i = a.Length - 1; i >= 0; i--)
                {
                    if (a[i].Equals(b[j]))
                        operations[i, j] = operations[i + 1, j + 1];
                    else
                        operations[i, j] = Math.Min(operations[i + 1, j] + 1, operations[i, j + 1] + 1);
                }
            return operations;
        }

        private List<IChange<T[]>> str_diff()
        {
            var diff = min_diff();
            diff.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            var changes = new List<IChange<T[]>>();
            int pos_diff = 0;
            int offset_b = 0;

            while (pos_diff < diff.Count)
            {
                int length = 0;
                int pos_a_old = diff[pos_diff].Item1;

                while (pos_diff < diff.Count && diff[pos_diff].Item2 == ChangeType.Insertion)
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

                    T[] sub = b.GetRange(r);
                    changes.Add(new Insert<T[]>(sub, pos_a, r));
                    offset_b += length;
                }
                if (pos_diff >= diff.Count)
                    break;
                length = 0;
                pos_a_old = diff[pos_diff].Item1;
                while (pos_diff < diff.Count && diff[pos_diff].Item2 == ChangeType.Deletion)
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

                    T[] sub = a.GetRange(r);
                    changes.Add(new Delete<T[]>(sub, pos_b, r));
                    offset_b -= length;
                }
            }

            return changes;
        }
    }
}
