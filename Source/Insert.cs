using System;

namespace DeadDog.Merging
{
    public class Insert<T> : Change<T>
    {
        private T value;
        private int pos;
        private Range range;

        public Insert(T value, int pos, Range range)
            : base(ChangeType.Insertion, value, pos, range)
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
            return string.Format("Delete(\"{0}\", {2}, {1})", value, pos, range);
        }
    }
}
