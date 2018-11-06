using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        private static IEnumerable<IChange<T>> WithMoves(IImmutableList<IChange<T>> changes, bool first, IMoveIdentifier<T> moveIdentifier)
        {
            var inserts = changes.OfType<Insert<T>>().ToImmutableList();
            var deletes = changes.OfType<Delete<T>>().ToImmutableList();
            var moves = ImmutableList<Move<T>>.Empty;

            for (int i = 0; i < inserts.Count; i++)
                for (int d = 0; d < deletes.Count; d++)
                {
                    var weight = moveIdentifier.MoveWeight(deletes[i], inserts[i]);
                    if (weight.HasValue)
                    {
                        moves = moves.Add(new Move<T>(deletes[d], inserts[i], first));
                        inserts = inserts.RemoveAt(i--);
                        deletes = deletes.RemoveAt(d--);

                        break;
                    }
                }

            return ImmutableList<IChange<T>>.Empty
                .AddRange(deletes)
                .AddRange(inserts)
                .AddRange(moves);
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

                case Move<T> move:
                    resolveConflict(move, b, cm);
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

                case Move<T> move:
                    // Delete actions that overlap with but are not fully contained within PsuedoMove sources collide
                    if (!move.From.OldRange.Contains(a.OldRange))
                    { }
                    else if (a.OldRange.Contains(move.From.OldRange, includeStart: false))
                        cm.AddConflict("[A] is deleting text that [B] is moving.");
                    else if (a.OldRange.OverlapsWith(move.From.OldRange))
                        cm.AddConflict("[B] is moving only part of some text that [A] is deleting.");

                    // Move destinations inside the range of Delete actions collide
                    if (a.OldRange.Contains(move.To.OldRange, includeStart: false))
                        cm.AddConflict("[A] is deleting text that [B] is moving text into.");
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

                case Move<T> move:
                    // Insert actions at the same location as Move destinations collide unless the text is the same
                    if (a.NewRange.Start == move.To.NewRange.Start)
                        if (a.Value.Equals(move.To.Value))
                            cm.RemoveA = true;
                        else
                            cm.AddConflict("[A] is inserting text at the same location that [B] is moving text to.");
                    break;

                default:
                    throw new ArgumentException($"Unknown change type: {b.GetType().Name}.");
            }
        }
        private void resolveConflict(Move<T> a, IChange<T> b, ConflictManager cm)
        {
            switch (b)
            {
                case Delete<T> delete:
                    resolveConflict(delete, a, cm);
                    cm.Swap();
                    break;

                case Insert<T> insert:
                    resolveConflict(insert, a, cm);
                    cm.Swap();
                    break;

                case Move<T> move:
                    // PsuedoMove actions collide if their source ranges overlap unless one is fully contained in the other
                    if (a.From.OldRange.OverlapsWith(move.From.OldRange))
                        if (!(a.From.OldRange.Contains(move.From.OldRange) || move.From.OldRange.Contains(a.From.OldRange)))
                            cm.AddConflict("A text move by [A] overlaps with a text move by [B].");

                    // Move actions collide if their destination positions are the same
                    if (a.To.NewRange == move.To.NewRange)
                        cm.AddConflict("[A] && [B] are moving text to the same location.");
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

            diff_a = WithMoves(diff_a, true, moveIdentifier).ToImmutableList();
            diff_b = WithMoves(diff_b, false, moveIdentifier).ToImmutableList();

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

            // sort the actions by position in the common ancestor
            ChangeQueue<T> actions = new ChangeQueue<T>(diff_a, diff_b);

            // compute offset lists
            var offset_changes_ab = OffsetManager.ConstructNoMove(actions);
            var offset_changes_a = OffsetManager.Construct(diff_a);
            var offset_changes_b = OffsetManager.Construct(diff_b);

            // compute the preliminary merge
            var preliminary_merge = ancestor.ToImmutableList();
            int pos_offset = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is Delete<T>)
                {
                    var pre = preliminary_merge.GetRange(0, actions[i].OldRange.Start + pos_offset);
                    var post = preliminary_merge.GetRange(actions[i].OldRange.End + pos_offset);

                    preliminary_merge = pre.AddRange(post);
                    pos_offset -= actions[i].OldRange.Length;
                    offset_changes_ab.AddOffset(actions[i].OldRange.Start, -actions[i].OldRange.Length);
                }
                else if (actions[i] is Insert<T>)
                {
                    var pre = preliminary_merge.GetRange(0, actions[i].OldRange.Start + pos_offset);
                    var post = preliminary_merge.GetRange(actions[i].OldRange.End + pos_offset);

                    preliminary_merge = pre.AddRange(actions[i].Value).AddRange(post);
                    pos_offset += actions[i].Value.Count;
                    offset_changes_ab.AddOffset(actions[i].OldRange.Start, actions[i].Value.Count);
                }
            }

            // perform the "delete" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i] is Move<T> move)
                {
                    Range range = offset_changes_ab.Offset(actions[i].OldRange);

                    offset_changes_ab.AddOffset(actions[i].OldRange.Start, -actions[i].OldRange.Length);
                    var pre = preliminary_merge.GetRange(0, range.Start);
                    var post = preliminary_merge.GetRange(range.End);
                    preliminary_merge = pre.AddRange(post);
                }

            // perform the "add" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i] is Move<T> move)
                {
                    int pos_a = offset_changes_ab.Offset(move.To.OldRange.Start);
                    var text_ancestor = actions[i].Value;
                    IImmutableList<T> text_a, text_b;
                    if (move.First)
                    {
                        text_a = move.To.Value;
                        var range = offset_changes_b.Offset(move.From.OldRange);
                        text_b = b.GetRange(range.Start, range.End);
                    }
                    else
                    {
                        text_b = move.To.Value;
                        var range = offset_changes_a.Offset(actions[i].OldRange);
                        text_a = a.GetRange(range.Start, range.End);
                    }
                    var text = merge(text_a, text_b, text_ancestor);
                    offset_changes_ab.AddOffset(actions[i].NewRange.Start, text.Count);

                    var pre = preliminary_merge.GetRange(0, pos_a);
                    var post = preliminary_merge.GetRange(pos_a);

                    preliminary_merge = pre.AddRange(text).AddRange(post);
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
            return new Merger<T>(null, null).merge(ancestor.ToImmutableList(), a.ToImmutableList(), b.ToImmutableList()).ToArray();
        }
    }
}
