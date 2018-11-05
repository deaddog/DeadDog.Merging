using System;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public class Delete<T> : IChange<T>
    {
        public Delete(IImmutableList<T> value, int position, Range range)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Position = position;
            Range = range;
        }

        public IImmutableList<T> Value { get; }

        public int Position { get; set; }
        public Range Range { get; set; }

        public override string ToString() => $@"Delete (""{Value}"", {Range}, {Position})";
    }
}
