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
    }
}
