using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public struct Range
    {
        private int start, end;

        public Range(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public static Range FromStartLength(int start, int length)
        {
            return new Range(start, start + length);
        }

        public bool OverlapsWith(Range range)
        {
            return (this.start <= range.start && this.end >= range.start) ||
                   (this.start <= range.end && this.end >= range.end) ||
                   range.Contains(this);
        }
        public bool Contains(Range range, bool includeStart = true)
        {
            return (includeStart ? (this.start <= range.start) : (this.start < range.start)) && this.end >= range.end;
        }
        public bool Contains(int position, bool includeStart = true)
        {
            return (includeStart ? (this.start <= position) : (this.start < position)) && this.end >= position;
        }

        public static Range Join(Range range1, Range range2)
        {
            return new Range(Math.Min(range1.start, range2.start), Math.Max(range1.end, range2.end));
        }

        public int Start
        {
            get { return start; }
        }
        public int End
        {
            get { return end; }
        }
        public int Length
        {
            get { return end - start; }
        }

        public override string ToString()
        {
            return "(" + start + ", " + end + ")";
        }
    }
}
