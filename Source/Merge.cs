using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class Merge
    {
        public static int leventhian<T>(T[] a, T[] b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++)
                d[i, 0] = i;
            for (int i = 0; i <= b.Length; i++)
                d[0, i] = i;

            for (int j = 1; j <= b.Length; j++)
                for (int i = 1; i <= a.Length; i++)
                    if (a[i - 1].Equals(b[j - 1]))
                        d[i, j] = d[i - 1, j - 1];
                    else
                        d[i, j] = Math.Min(Math.Min(d[i - 1, j], d[i, j - 1]), d[i - 1, j - 1]) + 1;
            return d[a.Length, b.Length];
        }
        // the maximum normalized distance (0-1) between two strings for them to be considered the same
        // for the purposes of finding Move actions
        const double MAX_MOVE_DIST = 0.2;

        // the minimum number of items that can be considered a Move action
        const int MIN_MOVE_LENGTH = 10;

        // represents moving <text_a> in range <range_a> to <text_b> in range <range_b>
        private class Move<T> : Change<T>
        {
            private T value2;
            private Range range2;
            private int pos2;

            private bool first;

            public Move(T value_a, Range range_a, int pos_a, T value_b, Range range_b, int pos_b, bool first)
                : base(ChangeType.Move, value_a, pos_a, range_a)
            {
                this.value2 = value_b;
                this.range2 = range_b;
                this.pos2 = pos_b;

                this.first = first;
            }

            public T Value2
            {
                get { return value2; }
            }
            public Range Range2
            {
                get { return range2; }
            }
            public int Position2
            {
                get { return pos2; }
            }

            public bool First
            {
                get { return first; }
            }
        }

        // find Move actions in a list of Change objects (mutates the input list).
        // a Move action comes from an Insert-Delete pair where the strings differ
        // by less than MAX_MOVE_DIST in terms of normalized Levenshtein distance
        public static void find_moves<K>(List<Change<K[]>> diff, bool first)
        {
            List<int> indices_to_delete = new List<int>();

            for (int i = 0; i < diff.Count; i++)
                if (diff[i].ChangeType == ChangeType.Deletion)
                    for (int j = 0; j < diff.Count; j++)
                        if (diff[j].ChangeType == ChangeType.Insertion)
                            if (!indices_to_delete.Contains(i) && !indices_to_delete.Contains(j))
                            {
                                double normalized_dist = leventhian(diff[i].Value, diff[j].Value) / Math.Max(diff[i].Value.Length, diff[j].Value.Length);
                                if (normalized_dist >= MAX_MOVE_DIST && Math.Max(diff[i].Value.Length, diff[j].Value.Length) >= MIN_MOVE_LENGTH)
                                {
                                    indices_to_delete.Add(i);
                                    indices_to_delete.Add(j);

                                    diff.Add(new Move<K[]>(diff[i].Value, diff[i].Range, diff[j].Position, diff[j].Value, diff[j].Range, diff[i].Position, first));
                                }
                            }
            indices_to_delete.Sort();
            indices_to_delete.Reverse();
            foreach (var i in indices_to_delete)
                diff.RemoveAt(i);
        }

        public static string merge(string ancestor, string a, string b)
        {
            // compute the diffs from the common ancestor
            var diff_a = OptimalDiff<char>.Diff(ancestor, a);
            var diff_b = OptimalDiff<char>.Diff(ancestor, b);

            // find Move actions
            find_moves(diff_a, true);
            find_moves(diff_b, false);

            // find conflicts and automatically resolve them where possible
            var conflicts = new List<string>();
            List<int> indices_to_delete_a = new List<int>();
            List<int> indices_to_delete_b = new List<int>();
            int len_diff_a = diff_a.Count;
            int len_diff_b = diff_b.Count;

            #region HandleCases

            for (int i = 0; i < len_diff_a; i++)
                for (int j = 0; j < len_diff_b; j++)
                {
                    if (indices_to_delete_b.Contains(j))
                        continue;

                    switch (diff_a[i].ChangeType)
                    {
                        case ChangeType.Deletion: switch (diff_b[j].ChangeType)
                            {
                                case ChangeType.Deletion:
                                    // if two Delete actions overlap, take the union of their ranges
                                    if ((diff_b[j].Range.Start >= diff_a[i].Range.Start && diff_b[j].Range.Start < diff_a[i].Range.End) ||
                                        (diff_b[j].Range.End >= diff_a[i].Range.Start && diff_b[j].Range.End < diff_a[i].Range.End) ||
                                        (diff_b[j].Range.Start < diff_a[i].Range.Start && diff_b[j].Range.End > diff_a[i].Range.End))
                                    {
                                        diff_a[i].Range = new Range(Math.Min(diff_a[i].Range.Start, diff_b[j].Range.Start), Math.Max(diff_a[i].Range.End, diff_b[j].Range.End));
                                        indices_to_delete_b.Add(j);
                                    }
                                    break;
                                case ChangeType.Insertion:
                                    // Insert actions inside the range of Delete actions collide
                                    if (diff_b[j].Position > diff_a[i].Range.Start && diff_b[j].Position < diff_a[i].Range.End)
                                        conflicts.Add("A is deleting text that B is inserting into.");
                                    break;
                                case ChangeType.Move:
                                    // Delete actions that overlap with but are not fully contained within PsuedoMove sources collide
                                    if (diff_a[i].Range.Start >= diff_b[j].Range.Start && diff_a[i].Range.End <= diff_b[j].Range.End)
                                    { }
                                    else if (diff_a[i].Range.Start >= diff_b[j].Range.Start && diff_a[i].Range.Start < diff_b[j].Range.End)
                                        conflicts.Add("B is moving only part of some text that A is deleting.");
                                    else if (diff_a[i].Range.End >= diff_b[j].Range.Start && diff_a[i].Range.End < diff_b[j].Range.End)
                                        conflicts.Add("B is moving only part of some text that A is deleting.");
                                    else if (diff_a[i].Range.Start < diff_b[j].Range.Start && diff_a[i].Range.End > diff_b[j].Range.End)
                                        conflicts.Add("A is deleting text that B is moving.");
                                    // Move destinations inside the range of Delete actions collide
                                    if (diff_b[j].Position > diff_a[i].Range.Start && diff_b[j].Position < diff_a[i].Range.End)
                                        conflicts.Add("A is deleting text that B is moving text into.");
                                    break;
                            } break;
                        case ChangeType.Insertion: switch (diff_b[j].ChangeType)
                            {
                                case ChangeType.Deletion:
                                    // Insert actions inside the range of Delete actions collide
                                    if (diff_a[i].Position > diff_b[j].Range.Start && diff_a[i].Position < diff_b[j].Range.End)
                                        conflicts.Add("B is deleting text that A is inserting into.");
                                    break;
                                case ChangeType.Insertion:
                                    // Insert actions at the same position collide unless the inserted text is the same
                                    if (diff_a[i].Position == diff_b[j].Position)
                                        if (diff_a[i].Value.Equals(diff_b[j].Value))
                                            indices_to_delete_b.Add(j);
                                        else
                                            conflicts.Add("A && B are inserting text at the same location.");
                                    break;
                                case ChangeType.Move:
                                    // Insert actions at the same location as Move destinations collide unless the text is the same
                                    if (diff_a[i].Position == diff_b[j].Position)
                                        if (diff_a[i].Value.Equals((diff_b[j] as Move<char[]>).Value2))
                                            indices_to_delete_a.Add(i);
                                        else
                                            conflicts.Add("A is inserting text at the same location that B is moving text to.");
                                    break;
                            } break;
                        case ChangeType.Move: switch (diff_b[j].ChangeType)
                            {
                                case ChangeType.Deletion:
                                    // Delete actions that overlap with but are not fully contained within PsuedoMove actions collide
                                    if (diff_b[j].Range.Start >= diff_a[i].Range.Start && diff_b[j].Range.End <= diff_a[i].Range.End)
                                    { }
                                    else if (diff_b[j].Range.Start >= diff_a[i].Range.Start && diff_b[j].Range.Start < diff_a[i].Range.End)
                                        conflicts.Add("A is moving only part of some text that B is deleting.");
                                    else if (diff_b[j].Range.End >= diff_a[i].Range.Start && diff_b[j].Range.End < diff_a[i].Range.End)
                                        conflicts.Add("A is moving only part of some text that B is deleting.");
                                    else if (diff_b[j].Range.Start < diff_a[i].Range.Start && diff_b[j].Range.End > diff_a[i].Range.End)
                                        conflicts.Add("B is deleting text that A is moving.");
                                    break;
                                case ChangeType.Insertion:
                                    // Insert actions at the same location as Move destinations collide unless the text is the same
                                    if (diff_b[j].Position == diff_a[i].Position)
                                        if (diff_b[j].Value.Equals((diff_a[i] as Move<char[]>).Value2))
                                            indices_to_delete_b.Add(j);
                                        else
                                            conflicts.Add("B is inserting text at the same location that A is moving text to.");
                                    break;
                                case ChangeType.Move:
                                    // PsuedoMove actions collide if their source ranges overlap unless one is fully contained in the other
                                    if (diff_b[j].Range.Start >= diff_a[i].Range.Start && diff_b[j].Range.End <= diff_a[i].Range.End)
                                    { }
                                    else if (diff_b[j].Range.Start >= diff_a[i].Range.Start && diff_b[j].Range.Start < diff_a[i].Range.End)
                                        conflicts.Add("A text move by A overlaps with a text move by B.");
                                    else if (diff_b[j].Range.End >= diff_a[i].Range.Start && diff_b[j].Range.End < diff_a[i].Range.End)
                                        conflicts.Add("A text move by A overlaps with a text move by B.");
                                    else if (diff_b[j].Range.Start < diff_a[i].Range.Start && diff_b[j].Range.End > diff_a[i].Range.End)
                                    { }
                                    // Move actions collide if their destination positions are the same
                                    if (diff_a[i].Position == diff_b[j].Position)
                                        conflicts.Add("A && B are moving text to the same location.");
                                    break;
                            } break;
                    }
                }

            #endregion

            indices_to_delete_a.Sort();
            indices_to_delete_a.Reverse();
            foreach (var i in indices_to_delete_a)
                diff_a.RemoveAt(i);
            indices_to_delete_b.Sort();
            indices_to_delete_b.Reverse();

            foreach (var i in indices_to_delete_b)
                diff_b.RemoveAt(i);
            // throw an error if there are conflicts
            if (conflicts.Count > 0)
                throw new Exception("CONFLICT!");
            // sort the actions by position in the common ancestor
            Func<Change<char[]>, int> sort_key = action =>
            {
                switch (action.ChangeType)
                {
                    case ChangeType.Deletion:
                        return action.Range.Start;
                    case ChangeType.Insertion:
                        return action.Position;
                    case ChangeType.Move:
                        return action.Position;
                    default: throw new InvalidCastException();
                }
            };
            List<Change<char[]>> actions = new List<Change<char[]>>();
            actions.AddRange(diff_a);
            actions.AddRange(diff_b);
            actions.Sort((x, y) => sort_key(x).CompareTo(sort_key(y)));

            // compute offset lists
            var offset_changes_ab = new List<Tuple<int, int>>();
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].ChangeType == ChangeType.Deletion)
                    offset_changes_ab.Add(Tuple.Create(actions[i].Range.Start, actions[i].Range.Start - actions[i].Range.End));
                if (actions[i].ChangeType == ChangeType.Insertion)
                    offset_changes_ab.Add(Tuple.Create(actions[i].Position, actions[i].Value.Length));
            }
            var offset_changes_a = new List<Tuple<int, int>>();
            for (int i = 0; i < diff_a.Count; i++)
                switch (diff_a[i].ChangeType)
                {
                    case ChangeType.Deletion:
                        offset_changes_a.Add(Tuple.Create(diff_a[i].Range.Start, diff_a[i].Range.Start - diff_a[i].Range.End));
                        break;
                    case ChangeType.Insertion:
                        offset_changes_a.Add(Tuple.Create(diff_a[i].Position, diff_a[i].Value.Length));
                        break;
                    case ChangeType.Move:
                        offset_changes_a.Add(Tuple.Create(diff_a[i].Range.Start, diff_a[i].Range.Start - diff_a[i].Range.End));
                        offset_changes_a.Add(Tuple.Create(diff_a[i].Position, diff_a[i].Value.Length));
                        break;
                }
            var offset_changes_b = new List<Tuple<int, int>>();
            for (int i = 0; i < diff_b.Count; i++)
                switch (diff_b[i].ChangeType)
                {
                    case ChangeType.Deletion:
                        offset_changes_b.Add(Tuple.Create(diff_b[i].Range.Start, diff_b[i].Range.Start - diff_b[i].Range.End));
                        break;
                    case ChangeType.Insertion:
                        offset_changes_b.Add(Tuple.Create(diff_b[i].Position, diff_b[i].Value.Length));
                        break;
                    case ChangeType.Move:
                        {
                            offset_changes_b.Add(Tuple.Create(diff_b[i].Range.Start, diff_b[i].Range.Start - diff_b[i].Range.End));
                            offset_changes_b.Add(Tuple.Create(diff_b[i].Position, diff_b[i].Value.Length));
                        }
                        break;
                }

            // compute the preliminary merge
            string preliminary_merge = (string)ancestor.Clone();
            int pos_offset = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].ChangeType == ChangeType.Deletion)
                {
                    preliminary_merge = preliminary_merge.Substring(0, actions[i].Range.Start + pos_offset) + preliminary_merge.Substring(actions[i].Range.End + pos_offset);
                    pos_offset += actions[i].Range.Start - actions[i].Range.End;
                    offset_changes_ab.Add(Tuple.Create(actions[i].Range.Start, actions[i].Range.Start - actions[i].Range.End));
                }
                if (actions[i].ChangeType == ChangeType.Insertion)
                {
                    preliminary_merge = preliminary_merge.Substring(0, actions[i].Position + pos_offset) + new string(actions[i].Value) + preliminary_merge.Substring(actions[i].Position + pos_offset);
                    pos_offset += actions[i].Value.Length;
                    offset_changes_ab.Add(Tuple.Create(actions[i].Position, actions[i].Value.Length));
                }
            }

            // perform the "delete" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i].ChangeType == ChangeType.Move)
                {
                    int range_a0 = actions[i].Range.Start;
                    int range_a1 = actions[i].Range.End;
                    foreach (var offset_pair in offset_changes_ab)
                    {
                        if (offset_pair.Item1 <= actions[i].Range.Start)
                            range_a0 += offset_pair.Item2;
                        if (offset_pair.Item1 <= actions[i].Range.End)
                            range_a1 += offset_pair.Item2;
                    }
                    offset_changes_ab.Add(Tuple.Create(actions[i].Range.Start, actions[i].Range.Start - actions[i].Range.End));
                    preliminary_merge = preliminary_merge.Substring(0, range_a0) + preliminary_merge.Substring(range_a1);
                }

            // perform the "add" part of the moves
            for (int i = 0; i < actions.Count; i++)
                if (actions[i].ChangeType == ChangeType.Move)
                {
                    var m = actions[i] as Move<char[]>;
                    int pos_a = actions[i].Position;
                    foreach (var offset_pair in offset_changes_ab)
                    {
                        if (offset_pair.Item1 <= actions[i].Position)
                            pos_a += offset_pair.Item2;
                    }
                    var text_ancestor = actions[i].Value;
                    char[] text_a, text_b;
                    if (m.First)
                    {
                        text_a = m.Value2;
                        var range_a0 = m.Range.Start;
                        var range_a1 = m.Range.End;
                        foreach (var offset_pair in offset_changes_b)
                        {
                            if (offset_pair.Item1 <= actions[i].Range.Start)
                                range_a0 += offset_pair.Item2;
                            if (offset_pair.Item1 <= actions[i].Range.End)
                                range_a1 += offset_pair.Item2;
                        }
                        text_b = b.Substring(range_a0, range_a1).ToCharArray();
                    }
                    else
                    {
                        text_b = m.Value2;
                        var range_a0 = actions[i].Range.Start;
                        var range_a1 = actions[i].Range.End;
                        foreach (var offset_pair in offset_changes_a)
                        {
                            if (offset_pair.Item1 <= actions[i].Range.Start)
                                range_a0 += offset_pair.Item2;
                            if (offset_pair.Item1 <= actions[i].Range.End)
                                range_a1 += offset_pair.Item2;
                        }
                        text_a = a.Substring(range_a0, range_a1).ToCharArray();
                    }
                    var text = merge(new string(text_a), new string(text_b), new string(text_ancestor));
                    offset_changes_ab.Add(Tuple.Create(actions[i].Position, text.Length));
                    preliminary_merge = preliminary_merge.Substring(0, pos_a) + text + preliminary_merge.Substring(pos_a);
                }
            return preliminary_merge;
        }
    }
}
