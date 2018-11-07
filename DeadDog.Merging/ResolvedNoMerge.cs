using System;

namespace DeadDog.Merging
{
    public class ResolvedNoMerge<T> : IResolved<T>
    {
        public ResolvedNoMerge(IChange<T> sourceA, IChange<T> sourceB)
        {
            ChangeA = sourceA ?? throw new ArgumentNullException(nameof(sourceA));
            ChangeB = sourceB ?? throw new ArgumentNullException(nameof(sourceB));
        }

        public IChange<T> ChangeA { get; }
        public IChange<T> ChangeB { get; }

        bool IResolved<T>.Success => true;
    }
}
