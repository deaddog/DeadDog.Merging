using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public interface IChange<T>
    {
        IImmutableList<T> Value { get; }

        Range OldRange { get; }
        Range NewRange { get; }
    }
}
