using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public abstract class Change<T>
    {
        private ChangeType type;
        private T value;
        private int pos;
        private Range range;

        public abstract Change<T2> Clone<T2>(T2 newValue);

        public Change(ChangeType type, T value, int pos, Range range)
        {
            this.type = type;
            this.value = value;
            this.pos = pos;
            this.range = range;
        }

        public static int AncestorPositionSort<T>(Change<T> a, Change<T> b)
        {
            return a.getAncestorKey().CompareTo(b.getAncestorKey());
        }
        internal abstract int getAncestorKey();

        public ChangeType ChangeType
        {
            get { return type; }
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
    }
}
