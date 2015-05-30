using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class Merge
    {
        // the maximum normalized distance (0-1) between two strings for them to be considered the same
        // for the purposes of finding Move actions
        const double MAX_MOVE_DIST = 0.2;

        // the minimum number of items that can be considered a Move action
        const int MIN_MOVE_LENGTH = 10;

        // find Move actions in a list of Change objects (mutates the input list).
        // a Move action comes from an Insert-Delete pair where the strings differ
        // by less than MAX_MOVE_DIST in terms of normalized Levenshtein distance
        public static void find_moves<K>(List<IChange<K[]>> diff, bool first) where K : IEquatable<K>
        {
            diff.Sort((x, y) => x.GetType().Equals(y.GetType()) ? 0 : (x is Delete<K[]> ? -1 : 1));
            int firstInsert = diff.FindIndex(x => x is Insert<K[]>);
            int count = diff.Count;

            for (int i = 0; i < firstInsert; i++)
                for (int j = firstInsert; j < count; j++)
                {
                    double normalized_dist = EditDistance.GetDistance(diff[i].Value, diff[j].Value) / Math.Max(diff[i].Value.Length, diff[j].Value.Length);
                    if (normalized_dist >= MAX_MOVE_DIST && Math.Max(diff[i].Value.Length, diff[j].Value.Length) >= MIN_MOVE_LENGTH)
                    {
                        diff.Add(new Move<K[]>(diff[i].Value, diff[i].Range, diff[j].Position, diff[j].Value, diff[j].Range, diff[i].Position, first));

                        diff.RemoveAt(i--);
                        diff.RemoveAt(j--);
                        firstInsert--;
                        count -= 2;

                        break;
                    }
                }
        }

        #region Methods for handling different possible conflict cases

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

        private static void resolveConflict<T>(Delete<T[]> a, Delete<T[]> b, ConflictManager cm)
        {
            // if two Delete actions overlap, take the union of their ranges
            if ((b.Range.Start >= a.Range.Start && b.Range.Start < a.Range.End) ||
                (b.Range.End >= a.Range.Start && b.Range.End < a.Range.End) ||
                (b.Range.Start < a.Range.Start && b.Range.End > a.Range.End))
            {
                a.Range = new Range(Math.Min(a.Range.Start, b.Range.Start), Math.Max(a.Range.End, b.Range.End));
                cm.RemoveB = true;
            }
        }
        private static void resolveConflict<T>(Delete<T[]> a, Insert<T[]> b, ConflictManager cm)
        {
            // Insert actions inside the range of Delete actions collide
            if (b.Position > a.Range.Start && b.Position < a.Range.End)
                cm.AddConflict("A is deleting text that B is inserting into.");
        }
        private static void resolveConflict<T>(Delete<T[]> a, Move<T[]> b, ConflictManager cm)
        {
            // Delete actions that overlap with but are not fully contained within PsuedoMove sources collide
            if (a.Range.Start >= b.Range1.Start && a.Range.End <= b.Range1.End)
            { }
            else if (a.Range.Start >= b.Range1.Start && a.Range.Start < b.Range1.End)
                cm.AddConflict("B is moving only part of some text that A is deleting.");
            else if (a.Range.End >= b.Range1.Start && a.Range.End < b.Range1.End)
                cm.AddConflict("B is moving only part of some text that A is deleting.");
            else if (a.Range.Start < b.Range1.Start && a.Range.End > b.Range1.End)
                cm.AddConflict("A is deleting text that B is moving.");

            // Move destinations inside the range of Delete actions collide
            if (b.Position1 > a.Range.Start && b.Position1 < a.Range.End)
                cm.AddConflict("A is deleting text that B is moving text into.");
        }

        private static void resolveConflict<T>(Insert<T[]> a, Delete<T[]> b, ConflictManager cm)
        {
            // Insert actions inside the range of Delete actions collide
            if (a.Position > b.Range.Start && a.Position < b.Range.End)
                cm.AddConflict("B is deleting text that A is inserting into.");
        }
        private static void resolveConflict<T>(Insert<T[]> a, Insert<T[]> b, ConflictManager cm)
        {
            // Insert actions at the same position collide unless the inserted text is the same
            if (a.Position == b.Position)
                if (a.Value.Equals(b.Value))
                    cm.RemoveB = true;
                else
                    cm.AddConflict("A && B are inserting text at the same location.");
        }
        private static void resolveConflict<T>(Insert<T[]> a, Move<T[]> b, ConflictManager cm)
        {
            // Insert actions at the same location as Move destinations collide unless the text is the same
            if (a.Position == b.Position1)
                if (a.Value.Equals((b as Move<char[]>).Value2))
                    cm.RemoveA = true;
                else
                    cm.AddConflict("A is inserting text at the same location that B is moving text to.");
        }

        private static void resolveConflict<T>(Move<T[]> a, Delete<T[]> b, ConflictManager cm)
        {
            // Delete actions that overlap with but are not fully contained within PsuedoMove actions collide
            if (b.Range.Start >= a.Range1.Start && b.Range.End <= a.Range1.End)
            { }
            else if (b.Range.Start >= a.Range1.Start && b.Range.Start < a.Range1.End)
                cm.AddConflict("A is moving only part of some text that B is deleting.");
            else if (b.Range.End >= a.Range1.Start && b.Range.End < a.Range1.End)
                cm.AddConflict("A is moving only part of some text that B is deleting.");
            else if (b.Range.Start < a.Range1.Start && b.Range.End > a.Range1.End)
                cm.AddConflict("B is deleting text that A is moving.");
        }
        private static void resolveConflict<T>(Move<T[]> a, Insert<T[]> b, ConflictManager cm)
        {
            // Insert actions at the same location as Move destinations collide unless the text is the same
            if (b.Position == a.Position1)
                if (b.Value.Equals((a as Move<char[]>).Value2))
                    cm.RemoveB = true;
                else
                    cm.AddConflict("B is inserting text at the same location that A is moving text to.");
        }
        private static void resolveConflict<T>(Move<T[]> a, Move<T[]> b, ConflictManager cm)
        {
            // PsuedoMove actions collide if their source ranges overlap unless one is fully contained in the other
            if (b.Range1.Start >= a.Range1.Start && b.Range1.End <= a.Range1.End)
            { }
            else if (b.Range1.Start >= a.Range1.Start && b.Range1.Start < a.Range1.End)
                cm.AddConflict("A text move by A overlaps with a text move by B.");
            else if (b.Range1.End >= a.Range1.Start && b.Range1.End < a.Range1.End)
                cm.AddConflict("A text move by A overlaps with a text move by B.");
            else if (b.Range1.Start < a.Range1.Start && b.Range1.End > a.Range1.End)
            { }

            // Move actions collide if their destination positions are the same
            if (a.Position1 == b.Position1)
                cm.AddConflict("A && B are moving text to the same location.");
        }

        #endregion

        public static string merge(string ancestor, string a, string b)
        {
            return new string(merge(ancestor.ToCharArray(), a.ToCharArray(), b.ToCharArray()));
        }
        public static T[] merge<T>(T[] ancestor, T[] a, T[] b) where T : IEquatable<T>
        {
            // compute the diffs from the common ancestor
            var diff_a = OptimalDiff<T>.Diff(ancestor, a);
            var diff_b = OptimalDiff<T>.Diff(ancestor, b);

            // find Move actions
            find_moves(diff_a, true);
            find_moves(diff_b, false);

            // find conflicts and automatically resolve them where possible
            var conflicts = new List<string>();

            for (int i = 0; i < diff_a.Count; i++)
                for (int j = 0; j < diff_b.Count; j++)
                {
                    ConflictManager cm = new ConflictManager();
                    resolveConflict((dynamic)diff_a[i], (dynamic)diff_b[j], cm);

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

            // sort the actions by position in the common ancestor
            ChangeQueue<T[]> actions = new ChangeQueue<T[]>(diff_a, diff_b);

            // compute offset lists
            var offset_changes_ab = Offset.ConstructNoMove(actions);
            var offset_changes_a = Offset.Construct(diff_a);
            var offset_changes_b = Offset.Construct(diff_b);

            // compute the preliminary merge
            T[] preliminary_merge = (T[])ancestor.Clone();
            int pos_offset = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is Delete<T[]>)
                {
                    preliminary_merge = preliminary_merge.GetRange(new Range(0, actions[i].Range.Start + pos_offset), actions[i].Range.End + pos_offset);
                    pos_offset += actions[i].Range.Start - actions[i].Range.End;
                    offset_changes_ab.AddOffset(actions[i].Range.Start, actions[i].Range.Start - actions[i].Range.End);
                }
                else if (actions[i] is Insert<T[]>)
                {
                    preliminary_merge = preliminary_merge.GetRange(new Range(0, actions[i].Position + pos_offset), actions[i].Value, actions[i].Position + pos_offset);
                    pos_offset += actions[i].Value.Length;
                    offset_changes_ab.AddOffset(actions[i].Position, actions[i].Value.Length);
                }
            }

            // perform the "delete" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i] is Move<T[]>)
                {
                    int range_a0 = offset_changes_ab.OffsetPosition(actions[i].Range.Start);
                    int range_a1 = offset_changes_ab.OffsetPosition(actions[i].Range.End);

                    offset_changes_ab.AddOffset(actions[i].Range.Start, actions[i].Range.Start - actions[i].Range.End);
                    preliminary_merge = preliminary_merge.GetRange(new Range(0, range_a0), range_a1);
                }

            // perform the "add" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i] is Move<T[]>)
                {
                    var m = actions[i] as Move<T[]>;
                    int pos_a = offset_changes_ab.OffsetPosition(actions[i].Position);
                    var text_ancestor = actions[i].Value;
                    T[] text_a, text_b;
                    if (m.First)
                    {
                        text_a = m.Value2;
                        var range_a0 = offset_changes_b.OffsetPosition(m.Range1.Start);
                        var range_a1 = offset_changes_b.OffsetPosition(m.Range1.End);
                        text_b = b.GetRange(new Range(range_a0, range_a1));
                    }
                    else
                    {
                        text_b = m.Value2;
                        var range_a0 = offset_changes_a.OffsetPosition(actions[i].Range.Start);
                        var range_a1 = offset_changes_a.OffsetPosition(actions[i].Range.End);
                        text_a = a.GetRange(new Range(range_a0, range_a1));
                    }
                    var text = merge(text_a, text_b, text_ancestor);
                    offset_changes_ab.AddOffset(actions[i].Position, text.Length);
                    preliminary_merge = preliminary_merge.GetRange(new Range(0, pos_a), text, pos_a);
                }
            return preliminary_merge;
        }
    }
}
