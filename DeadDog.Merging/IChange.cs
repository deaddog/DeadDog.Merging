using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public interface IChange<T>
    {
        IImmutableList<T> Value { get; }

        int Position { get; }
        Range Range { get; }
    }
}
