using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class Offset
    {
        private List<Tuple<int, int>> offsets;

        public Offset()
        {
            this.offsets = new List<Tuple<int, int>>();
        }

        public void AddOffset(int a, int b)
        {
            this.offsets.Add(Tuple.Create(a, b));
        }

        public int OffsetPosition(int position)
        {
            int pos = position;
            foreach (var offset_pair in offsets)
            {
                if (offset_pair.Item1 <= position)
                    pos += offset_pair.Item2;
            }
            return pos;
        }

        public static Offset Construct<T>(IEnumerable<IChange<T[]>> collection)
        {
            var offset_changes = new Offset();

            foreach (var change in collection)
            {
                if (!(change is Insert<T[]>))
                    offset_changes.AddOffset(change.Range.Start, change.Range.Start - change.Range.End);

                if (!(change is Delete<T[]>))
                    offset_changes.AddOffset(change.Position, change.Value.Length);
            }

            return offset_changes;
        }
        public static Offset ConstructNoMove<T>(IEnumerable<IChange<T[]>> collection)
        {
            var offset_changes = new Offset();

            foreach (var change in collection)
            {
                if (change is Delete<T[]>)
                    offset_changes.AddOffset(change.Range.Start, change.Range.Start - change.Range.End);

                if (change is Insert<T[]>)
                    offset_changes.AddOffset(change.Position, change.Value.Length);
            }

            return offset_changes;
        }
    }
}
