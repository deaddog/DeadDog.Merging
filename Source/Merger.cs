﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class Merger<T> where T : IEquatable<T>
    {
        private IMoveIdentifier<T> moveIdentifier;
        private IDiff<T> diffMethod;

        public Merger(IDiff<T> diffMethod = null, IMoveIdentifier<T> moveIdentifier = null)
        {
            if (diffMethod == null)
                diffMethod = new OptimalDiff<T>();
            this.diffMethod = diffMethod;

            if (moveIdentifier == null)
                moveIdentifier = new EditDistanceMoveIdentifier<T>();
            this.moveIdentifier = moveIdentifier;
        }

        // find Move actions in a list of Change objects (mutates the input list).
        public void find_moves<K>(List<IChange<K>> diff, bool first, IMoveIdentifier<K> identifier)
        {
            diff.Sort((x, y) => x.GetType().Equals(y.GetType()) ? 0 : (x is Delete<K> ? -1 : 1));
            int firstInsert = diff.FindIndex(x => x is Insert<K>);
            int count = diff.Count;

            for (int i = 0; i < firstInsert; i++)
                for (int j = firstInsert; j < count; j++)
                {
                    double? weight = identifier.MoveWeight(diff[i] as Delete<K>, diff[j] as Insert<K>);
                    if (weight.HasValue)
                    {
                        diff.Add(new Move<K>(diff[i].Value, diff[i].Range, diff[j].Position, diff[j].Value, diff[j].Range, diff[i].Position, first));

                        diff.RemoveAt(j--);
                        diff.RemoveAt(i--);
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

        private void resolveConflict(Delete<T> a, Delete<T> b, ConflictManager cm)
        {
            // if two Delete actions overlap, take the union of their ranges
            if (a.Range.OverlapsWith(b.Range))
            {
                a.Range = Range.Join(a.Range, b.Range);
                cm.RemoveB = true;
            }
        }
        private void resolveConflict(Delete<T> a, Insert<T> b, ConflictManager cm)
        {
            // Insert actions inside the range of Delete actions collide
            if (a.Range.Contains(b.Position, includeStart: false))
                cm.AddConflict("[A] is deleting text that [B] is inserting into.");
        }
        private void resolveConflict(Delete<T> a, Move<T> b, ConflictManager cm)
        {
            // Delete actions that overlap with but are not fully contained within PsuedoMove sources collide
            if (!b.Range1.Contains(a.Range))
            { }
            else if (a.Range.Contains(b.Range1, includeStart: false))
                cm.AddConflict("[A] is deleting text that [B] is moving.");
            else if (a.Range.OverlapsWith(b.Range1))
                cm.AddConflict("[B] is moving only part of some text that [A] is deleting.");

            // Move destinations inside the range of Delete actions collide
            if (a.Range.Contains(b.Position1, includeStart: false))
                cm.AddConflict("[A] is deleting text that [B] is moving text into.");
        }

        private void resolveConflict(Insert<T> a, Delete<T> b, ConflictManager cm)
        {
            resolveConflict(b, a, cm);
            cm.Swap();
        }
        private void resolveConflict(Insert<T> a, Insert<T> b, ConflictManager cm)
        {
            // Insert actions at the same position collide unless the inserted text is the same
            if (a.Position == b.Position)
                if (a.Value.Equals(b.Value))
                    cm.RemoveB = true;
                else
                    cm.AddConflict("[A] && [B] are inserting text at the same location.");
        }
        private void resolveConflict(Insert<T> a, Move<T> b, ConflictManager cm)
        {
            // Insert actions at the same location as Move destinations collide unless the text is the same
            if (a.Position == b.Position1)
                if (a.Value.Equals(b.Value2))
                    cm.RemoveA = true;
                else
                    cm.AddConflict("[A] is inserting text at the same location that [B] is moving text to.");
        }

        private void resolveConflict(Move<T> a, Delete<T> b, ConflictManager cm)
        {
            resolveConflict(b, a, cm);
            cm.Swap();
        }
        private void resolveConflict(Move<T> a, Insert<T> b, ConflictManager cm)
        {
            resolveConflict(b, a, cm);
            cm.Swap();
        }
        private void resolveConflict(Move<T> a, Move<T> b, ConflictManager cm)
        {
            // PsuedoMove actions collide if their source ranges overlap unless one is fully contained in the other
            if (a.Range1.OverlapsWith(b.Range1))
                if (!(a.Range1.Contains(b.Range1) || b.Range1.Contains(a.Range1)))
                    cm.AddConflict("A text move by [A] overlaps with a text move by [B].");

            // Move actions collide if their destination positions are the same
            if (a.Position1 == b.Position1)
                cm.AddConflict("[A] && [B] are moving text to the same location.");
        }

        #endregion

        public T[] merge(T[] ancestor, T[] a, T[] b)
        {
            // compute the diffs from the common ancestor
            var diff_a = diffMethod.Diff(ancestor, a).ToList();
            var diff_b = diffMethod.Diff(ancestor, b).ToList();

            // find Move actions
            find_moves(diff_a, true, moveIdentifier);
            find_moves(diff_b, false, moveIdentifier);

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
            ChangeQueue<T> actions = new ChangeQueue<T>(diff_a, diff_b);

            // compute offset lists
            var offset_changes_ab = OffsetManager.ConstructNoMove(actions);
            var offset_changes_a = OffsetManager.Construct(diff_a);
            var offset_changes_b = OffsetManager.Construct(diff_b);

            // compute the preliminary merge
            T[] preliminary_merge = (T[])ancestor.Clone();
            int pos_offset = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is Delete<T>)
                {
                    preliminary_merge = preliminary_merge.Subarray(0, actions[i].Range.Start + pos_offset) + preliminary_merge.Subarray(actions[i].Range.End + 1 + pos_offset);
                    pos_offset -= actions[i].Range.Length;
                    offset_changes_ab.AddOffset(actions[i].Range.Start, -actions[i].Range.Length);
                }
                else if (actions[i] is Insert<T>)
                {
                    preliminary_merge = preliminary_merge.Subarray(0, actions[i].Position + pos_offset) + actions[i].Value + preliminary_merge.Subarray(actions[i].Position + pos_offset);
                    pos_offset += actions[i].Value.Length;
                    offset_changes_ab.AddOffset(actions[i].Position, actions[i].Value.Length);
                }
            }

            // perform the "delete" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i] is Move<T>)
                {
                    Range range = offset_changes_ab.Offset(actions[i].Range);

                    offset_changes_ab.AddOffset(actions[i].Range.Start, -actions[i].Range.Length);
                    preliminary_merge = preliminary_merge.Subarray(0, range.Start) + preliminary_merge.Subarray(range.End + 1);
                }

            // perform the "add" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i] is Move<T>)
                {
                    var m = actions[i] as Move<T>;
                    int pos_a = offset_changes_ab.Offset(actions[i].Position);
                    var text_ancestor = actions[i].Value;
                    T[] text_a, text_b;
                    if (m.First)
                    {
                        text_a = m.Value2;
                        var range = offset_changes_b.Offset(m.Range1);
                        text_b = b.Subarray(range.Start, range.End + 1);
                    }
                    else
                    {
                        text_b = m.Value2;
                        var range = offset_changes_a.Offset(actions[i].Range);
                        text_a = a.Subarray(range.Start, range.End + 1);
                    }
                    var text = merge(text_a, text_b, text_ancestor);
                    offset_changes_ab.AddOffset(actions[i].Position, text.Length);
                    preliminary_merge = preliminary_merge.Subarray(0, pos_a) + text + preliminary_merge.Subarray(pos_a);
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
            return new Merger<T>(null, null).merge(ancestor, a, b);
        }
    }
}
