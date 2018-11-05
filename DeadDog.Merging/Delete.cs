using System;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public class Delete<T> : IChange<T>
    {
        public Delete(IImmutableList<T> value, Range deletedRange, int newPosition)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));

            OldRange = deletedRange;
            NewRange = Range.FromStartLength(newPosition, 0);
        }

        public IImmutableList<T> Value { get; }

        public Range OldRange { get; set; }
        public Range NewRange { get; }
    }
}
