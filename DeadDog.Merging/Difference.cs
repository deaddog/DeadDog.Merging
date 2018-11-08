using System.Collections.Generic;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public delegate IEnumerable<IChange<T>> Difference<T>(IImmutableList<T> sourceA, IImmutableList<T> sourceB);
}
