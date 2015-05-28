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
