using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class OffsetManager
    {
        private List<Tuple<int, int>> offsets;

        public OffsetManager()
        {
            this.offsets = new List<Tuple<int, int>>();
        }

        public void AddOffset(int a, int b)
        {
            this.offsets.Add(Tuple.Create(a, b));
        }

        public int Offset(int position)
        {
            int pos = position;
            foreach (var offset_pair in offsets)
            {
                if (offset_pair.Item1 <= position)
                    pos += offset_pair.Item2;
            }
            return pos;
        }
        public Range Offset(Range range)
        {
            return new Range(Offset(range.Start), Offset(range.End));
        }

        public static OffsetManager Construct<T>(IEnumerable<IChange<T>> collection)
        {
            var offset_changes = new OffsetManager();

            foreach (var change in collection)
            {
                if (!(change is Insert<T>))
                    offset_changes.AddOffset(change.NewRange.Start, -change.NewRange.Length);

                if (!(change is Delete<T>))
                    offset_changes.AddOffset(change.NewRange.Start, change.Value.Count);
            }

            return offset_changes;
        }
        public static OffsetManager ConstructNoMove<T>(IEnumerable<IChange<T>> collection)
        {
            var offset_changes = new OffsetManager();

            foreach (var change in collection)
            {
                if (change is Delete<T>)
                    offset_changes.AddOffset(change.OldRange.Start, -change.OldRange.Length);

                if (change is Insert<T>)
                    offset_changes.AddOffset(change.OldRange.Start, change.Value.Count);
            }

            return offset_changes;
        }
    }
}
