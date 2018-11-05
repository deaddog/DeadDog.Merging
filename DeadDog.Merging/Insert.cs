using System;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public class Insert<T> : IChange<T>
    {
        public Insert(IImmutableList<T> value, int position, Range range)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Position = position;
            Range = range;
        }

        public IImmutableList<T> Value { get; }

        public int Position { get; }
        public Range Range { get; }

        public override string ToString() => $@"Insert(""{Value}"", {Position}, {Range})";
    }
}
