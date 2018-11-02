using System;

namespace DeadDog.Merging
{
    public class Delete<T> : IChange<T>
    {
        private T[] value;
        private int pos;
        private Range range;

        public Delete<T2> Clone<T2>(T2[] newValue)
        {
            return new Delete<T2>(newValue, this.pos, this.range);
        }
        IChange<T2> IChange<T>.Clone<T2>(T2[] newValue)
        {
            return this.Clone(newValue);
        }

        public Delete(T[] value, int pos, Range range)
        {
            this.value = value;
            this.pos = pos;
            this.range = range;
        }

        public T[] Value
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
            return string.Format("Delete(\"{0}\", {2}, {1})", value, pos, range);
        }
    }
}
