using System;

namespace DeadDog.Merging
{
    // represents moving <text_a> in range <range_a> to <text_b> in range <range_b>
    public class Move<T> : IChange<T>
    {
        private T[] value1;
        private Range range1;
        private int pos1;

        private T[] value2;
        private Range range2;
        private int pos2;

        private bool first;

        IChange<T2> IChange<T>.Clone<T2>(T2[] newValue)
        {
            throw new InvalidOperationException("A move cannot be cloned using a single new value.");
        }
        public Move<T2> Clone<T2>(T2[] newValue1, T2[] newValue2)
        {
            return new Move<T2>(newValue1, range1, pos1, newValue2, range2, pos2, first);
        }

        public Move(T[] value_a, Range range_a, int pos_a, T[] value_b, Range range_b, int pos_b, bool first)
        {
            this.value1 = value_a;
            this.range1 = range_a;
            this.pos1 = pos_a;

            this.value2 = value_b;
            this.range2 = range_b;
            this.pos2 = pos_b;

            this.first = first;
        }

        T[] IChange<T>.Value
        {
            get { return value1; }
        }
        Range IChange<T>.Range
        {
            get { return range1; }
        }
        int IChange<T>.Position
        {
            get { return pos1; }
        }

        public T[] Value1
        {
            get { return value1; }
        }
        public Range Range1
        {
            get { return range1; }
        }
        public int Position1
        {
            get { return pos1; }
        }

        public T[] Value2
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

        public override string ToString()
        {
            return string.Format("Move(\"{0}\" -> \"{1}\")", value1, value2);
        }
    }
}
