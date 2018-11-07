using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DeadDog.Merging
{
    public class Merger<T> where T : IEquatable<T>
    {
        private IDiff<T> diffMethod;

        public Merger(IDiff<T> diffMethod = null)
        {
            if (diffMethod == null)
                diffMethod = new OptimalDiff<T>();
            this.diffMethod = diffMethod;
        }

        private class ConflictManager
        {
            private bool removeA;
            private bool removeB;
            private List<string> conflicts;

            public ConflictManager()
            {
                this.removeA = false;
                this.removeB = false;
                this.conflicts = new List<string>();
            }

            public void Swap()
            {
                bool temp = removeA;
                removeA = removeB;
                removeB = temp;

                for (int i = 0; i < conflicts.Count; i++)
                    conflicts[i] = conflicts[i].Replace("[A]", "[T]").Replace("[B]", "[A]").Replace("[T]", "[B]");
            }

            public bool RemoveA
            {
                get { return removeA; }
                set { removeA = value; }
            }
            public bool RemoveB
            {
                get { return removeB; }
                set { removeB = value; }
            }

            public void AddConflict(string conflict)
            {
                this.conflicts.Add(conflict);
            }

            public IEnumerable<string> GetConflicts()
            {
                foreach (var c in conflicts)
                    yield return c;
            }
        }

        private void resolveConflict(IChange<T> a, IChange<T> b, ConflictManager cm)
        {
            switch (a)
            {
                case Delete<T> delete:
                    resolveConflict(delete, b, cm);
                    break;

                case Insert<T> insert:
                    resolveConflict(insert, b, cm);
                    break;

                default:
                    throw new ArgumentException($"Unknown change type: {a.GetType().Name}.");
            }
        }

        private void resolveConflict(Delete<T> a, IChange<T> b, ConflictManager cm)
        {
            switch (b)
            {
                case Delete<T> delete:
                    // if two Delete actions overlap, take the union of their ranges
                    if (a.OldRange.OverlapsWith(delete.OldRange))
                    {
                        a.OldRange = Range.Join(a.OldRange, delete.OldRange);
                        cm.RemoveB = true;
                    }
                    break;

                case Insert<T> insert:
                    // Insert actions inside the range of Delete actions collide
                    if (a.OldRange.Contains(insert.OldRange, includeStart: false))
                        cm.AddConflict("[A] is deleting text that [B] is inserting into.");
                    break;

                default:
                    throw new ArgumentException($"Unknown change type: {b.GetType().Name}.");
            }
        }
        private void resolveConflict(Insert<T> a, IChange<T> b, ConflictManager cm)
        {
            switch (b)
            {
                case Delete<T> delete:
                    resolveConflict(delete, a, cm);
                    cm.Swap();
                    break;

                case Insert<T> insert:
                    // Insert actions at the same position collide unless the inserted text is the same
                    if (a.NewRange.Start == insert.NewRange.Start)
                        if (a.Value.Equals(insert.Value))
                            cm.RemoveB = true;
                        else
                            cm.AddConflict("[A] && [B] are inserting text at the same location.");
                    break;

                default:
                    throw new ArgumentException($"Unknown change type: {b.GetType().Name}.");
            }
        }

        public IImmutableList<T> merge(IImmutableList<T> ancestor, IImmutableList<T> a, IImmutableList<T> b)
        {
            // compute the diffs from the common ancestor
            var diff_a = diffMethod.Diff(ancestor, a).ToImmutableList();
            var diff_b = diffMethod.Diff(ancestor, b).ToImmutableList();

            // find conflicts and automatically resolve them where possible
            var conflicts = new List<string>();

            for (int i = 0; i < diff_a.Count; i++)
                for (int j = 0; j < diff_b.Count; j++)
                {
                    ConflictManager cm = new ConflictManager();
                    resolveConflict(diff_a[i], diff_b[j], cm);

                    conflicts.AddRange(cm.GetConflicts());

                    if (cm.RemoveB)
                        diff_b.RemoveAt(j--);
                    if (cm.RemoveA)
                    {
                        diff_a.RemoveAt(i--);
                        break;
                    }
                }

            // throw an error if there are conflicts
            if (conflicts.Count > 0)
                throw new Exception("CONFLICT!");

            var changes = diff_a.AddRange(diff_b).OrderBy(x => x.OldRange.Start).ToImmutableList();

            // compute the preliminary merge
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
            return new Merger<T>(null).merge(ancestor.ToImmutableList(), a.ToImmutableList(), b.ToImmutableList()).ToArray();
        }
    }
}
