using System;

namespace DeadDog.Merging
{
    public class ResolvedMerge<T> : IResolved<T>
    {
        public ResolvedMerge(IChange<T> sourceA, IChange<T> sourceB, IChange<T> merged)
        {
            SourceA = sourceA ?? throw new ArgumentNullException(nameof(sourceA));
            SourceB = sourceB ?? throw new ArgumentNullException(nameof(sourceB));
            Merged = merged ?? throw new ArgumentNullException(nameof(merged));
        }

        public IChange<T> SourceA { get; }
        public IChange<T> SourceB { get; }

        public IChange<T> Merged { get; }
        bool IResolved<T>.Success => true;
    }
}
