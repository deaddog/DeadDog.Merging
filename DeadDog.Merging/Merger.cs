using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DeadDog.Merging
{
    public class Merger<T> where T : IEquatable<T>
    {
        private IResolved<T> ResolveConflict(IChange<T> a, IChange<T> b)
        {
            switch (a)
            {
                case Delete<T> delete: return ResolveConflict(delete, b);
                case Insert<T> insert: return ResolveConflict(insert, b);

                default:
                    throw new ArgumentException($"Unknown change type: {a.GetType().Name}.");
            }
        }

        private IResolved<T> ResolveConflict(Delete<T> a, IChange<T> b)
        {
            switch (b)
            {
                case Delete<T> delete:
                    if (a.OldRange.OverlapsWith(delete.OldRange))
                    {
                        var value = a.Value;
                        var range = Range.Join(a.OldRange, delete.OldRange);

                        var newDelete = new Delete<T>
                        (
                            value: a.Value,
                            deletedRange: Range.Join(a.OldRange, delete.OldRange),
                            newPosition: a.NewRange.Start
                        );

                        return Resolved.AsMerge(a, delete, newDelete);
                    }
                    else
                        return Resolved.AsNoMerge(a, delete);

                case Insert<T> insert:
                    if (a.OldRange.Contains(insert.OldRange, includeStart: false))
                        return Resolved.AsConflict(a, insert, "[A] is deleting text that [B] is inserting into.");
                    else
                        return Resolved.AsNoMerge(a, insert);

                default:
                    throw new ArgumentException($"Unknown change type: {b.GetType().Name}.");
            }
        }
        private IResolved<T> ResolveConflict(Insert<T> a, IChange<T> b)
        {
            switch (b)
            {
                case Delete<T> delete:
                    if (delete.OldRange.Contains(a.OldRange, includeStart: false))
                        return Resolved.AsConflict(delete, a, "[B] is deleting text that [A] is inserting into.");
                    else
                        return Resolved.AsNoMerge(delete, a);

                case Insert<T> insert:
                    if (a.NewRange.Start == insert.NewRange.Start)
                        if (a.Value.Equals(insert.Value))
                            return Resolved.AsMerge(a, insert, a);
                        else
                            return Resolved.AsConflict(a, insert, "[A] && [B] are inserting text at the same location.");
                    else
                        return Resolved.AsNoMerge(a, insert);

                default:
                    throw new ArgumentException($"Unknown change type: {b.GetType().Name}.");
            }
        }

        public IImmutableList<T> merge(IImmutableList<T> ancestor, IImmutableList<T> a, IImmutableList<T> b)
        {
            var diff_a = EditDistance.GetDifference(ancestor, a).ToImmutableList();
            var diff_b = EditDistance.GetDifference(ancestor, b).ToImmutableList();

            for (int i = 0; i < diff_a.Count; i++)
                for (int j = 0; j < diff_b.Count; j++)
                {
                    var resolved = ResolveConflict(diff_a[i], diff_b[j]);

                    switch (resolved)
                    {
                        case UnResolvedMerge<T> conflict: throw new Exception("CONFLICT!");
                        case ResolvedNoMerge<T> noMerge: break;

                        case ResolvedMerge<T> merge:
                            {
                                diff_b = diff_b.RemoveAt(j--);
                                diff_a = diff_a.SetItem(i, merge.Merged);
                            };
                            break;

                        default: throw new NotSupportedException($"The type {resolved.GetType().Name} is not supported as a resolve type.");
                    }
                }

            var changes = diff_a.AddRange(diff_b).OrderBy(x => x.OldRange.Start).ToImmutableList();

            var preliminary_merge = ancestor.ToImmutableList();
            int pos_offset = 0;

            foreach (var c in changes)
                if (c is Delete<T> delete)
                {
                    var pre = preliminary_merge.GetRange(0, delete.OldRange.Start + pos_offset);
                    var post = preliminary_merge.GetRange(delete.OldRange.End + pos_offset);

                    preliminary_merge = pre.AddRange(post);
                    pos_offset -= delete.OldRange.Length;
                }
                else if (c is Insert<T> insert)
                {
                    var pre = preliminary_merge.GetRange(0, insert.OldRange.Start + pos_offset);
                    var post = preliminary_merge.GetRange(insert.OldRange.End + pos_offset);

                    preliminary_merge = pre.AddRange(insert.Value).AddRange(post);
                    pos_offset += insert.Value.Count;
                }

            return preliminary_merge;
        }
    }

    public static class Merger
    {
        public static string merge(string ancestor, string a, string b)
        {
            return new string(merge(ancestor.ToCharArray(), a.ToCharArray(), b.ToCharArray()));
        }
        public static T[] merge<T>(T[] ancestor, T[] a, T[] b) where T : IEquatable<T>
        {
            return new Merger<T>().merge(ancestor.ToImmutableList(), a.ToImmutableList(), b.ToImmutableList()).ToArray();
        }
    }
}
