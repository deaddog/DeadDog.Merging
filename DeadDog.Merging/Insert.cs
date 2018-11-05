using System;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public class Insert<T> : IChange<T>
    {
        public Insert(IImmutableList<T> value, int insertedAt, Range newRange)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));

            OldRange = Range.FromStartLength(start: insertedAt, length: 0);
            NewRange = newRange;
        }

        public IImmutableList<T> Value { get; }

        public Range OldRange { get; }
        public Range NewRange { get; }
    }
}
