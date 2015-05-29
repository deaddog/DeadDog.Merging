using System;

namespace DeadDog.Merging
{
    public class Insert<T> : IChange<T>
    {
        private T value;
        private int pos;
        private Range range;

        public Insert<T2> Clone<T2>(T2 newValue)
        {
            return new Insert<T2>(newValue, this.pos, this.range);
        }
        IChange<T2> IChange<T>.Clone<T2>(T2 newValue)
        {
            return this.Clone(newValue);
        }

        public Insert(T value, int pos, Range range)
        {
            this.value = value;
            this.pos = pos;
            this.range = range;
        }

        public T Value
        {
            get { return value; }
        }
        public int Position
        {
            get { return pos; }
        }
        public Range Range
        {
            get { return range; }
            set { range = value; }
        }

        public override string ToString()
        {
            return string.Format("Insert(\"{0}\", {1}, {2})", value, pos, range);
        }
    }
}
