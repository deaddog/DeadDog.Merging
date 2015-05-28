using System;

namespace DeadDog.Merging
{
    public class Insert<T> : Change<T>
    {
        private T value;
        private int pos;
        private Range range;

        public override Change<T2> Clone<T2>(T2 newValue)
        {
            return new Insert<T2>(newValue, this.pos, this.range);
        }

        public Insert(T value, int pos, Range range)
            : base(ChangeType.Insertion, value, pos, range)
        {
            this.value = value;
            this.pos = pos;
            this.range = range;
        }

        internal override int getAncestorKey()
        {
            return pos;
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
