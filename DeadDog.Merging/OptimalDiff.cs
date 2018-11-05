using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class OptimalDiff<T> : IDiff<T> where T : IEquatable<T>
    {
        public OptimalDiff()
        {
        }

        public IEnumerable<IChange<T>> Diff(IEnumerable<T> origin, IEnumerable<T> modified)
        {
            return str_diff(origin.ToImmutableList(), modified.ToImmutableList());
        }

        private List<IChange<T>> str_diff(IImmutableList<T> a, IImmutableList<T> b)
        {
            var diff = EditDistance.GetOperations(a, b);
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
                    changes.Add(new Delete<T>(sub, pos_b, r));
                    offset_b -= length;
                }
            }

            return changes;
        }

    }
}
