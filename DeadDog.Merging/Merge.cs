using System;
using System.Collections.Immutable;
using System.Linq;

namespace DeadDog.Merging
{
    public static class Merge<T>
    {
        public static IImmutableList<T> merge(IImmutableList<T> ancestor, IImmutableList<T> sourceA, IImmutableList<T> sourceB)
        {
            var diffA = EditDistance.GetDifference(ancestor, sourceA).ToImmutableList();
            var diffB = EditDistance.GetDifference(ancestor, sourceB).ToImmutableList();

            for (int i = 0; i < diffA.Count; i++)
                for (int j = 0; j < diffB.Count; j++)
                {
                    var resolved = ResolveConflict(diffA[i], diffB[j]);

                    switch (resolved)
                    {
                        case UnResolvedMerge<T> conflict: throw new Exception("CONFLICT!");
                        case ResolvedNoMerge<T> noMerge: break;

                        case ResolvedMerge<T> merge:
                            {
                                diffB = diffB.RemoveAt(j--);
                                diffA = diffA.SetItem(i, merge.Merged);
                            };
                            break;

                        default: throw new NotSupportedException($"The type {resolved.GetType().Name} is not supported as a resolve type.");
                    }
                }

            var changes = diffA.AddRange(diffB).OrderBy(x => x.OldRange.Start).ToImmutableList();

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
                else
                    throw new NotSupportedException($"The change type {c.GetType().Name} is not a supported change type.");

            return preliminary_merge;
        }

        private static IResolved<T> ResolveConflict(IChange<T> a, IChange<T> b)
        {
            switch (a)
            {
                case Delete<T> delete: return ResolveConflict(delete, b);
                case Insert<T> insert: return ResolveConflict(insert, b);

                default:
                    throw new ArgumentException($"Unknown change type: {a.GetType().Name}.");
            }
        }

        private static IResolved<T> ResolveConflict(Delete<T> a, IChange<T> b)
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
        private static IResolved<T> ResolveConflict(Insert<T> a, IChange<T> b)
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
    }

    public static class Merge
    {
        public static string merge(string ancestor, string a, string b)
        {
            return new string(merge(ancestor.ToCharArray(), a.ToCharArray(), b.ToCharArray()));
        }
        public static T[] merge<T>(T[] ancestor, T[] a, T[] b)
        {
            return Merge<T>.merge(ancestor.ToImmutableList(), a.ToImmutableList(), b.ToImmutableList()).ToArray();
        }
    }
}
