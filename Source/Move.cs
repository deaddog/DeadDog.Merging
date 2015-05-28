using System;

namespace DeadDog.Merging
{
    // represents moving <text_a> in range <range_a> to <text_b> in range <range_b>
    public class Move<T> : Change<T>
    {
        private T value2;
        private Range range2;
        private int pos2;

        private bool first;

        public override Change<T2> Clone<T2>(T2 newValue)
        {
            throw new InvalidOperationException("A move cannot be cloned using a single new value.");
        }
        public Move<T2> Clone<T2>(T2 newValue1, T2 newValue2)
        {
            return new Move<T2>(newValue1, Range, Position, newValue2, range2, pos2, first);
        }

        public Move(T value_a, Range range_a, int pos_a, T value_b, Range range_b, int pos_b, bool first)
            : base(ChangeType.Move, value_a, pos_a, range_a)
        {
            this.value2 = value_b;
            this.range2 = range_b;
            this.pos2 = pos_b;

            this.first = first;
        }

        internal override int getAncestorKey()
        {
            return Position;
        }

        public T Value2
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
    }
}
