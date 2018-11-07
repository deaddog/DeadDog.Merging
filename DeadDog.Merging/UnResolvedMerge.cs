using System;

namespace DeadDog.Merging
{
    public class UnResolvedMerge<T> : IResolved<T>
    {
        public UnResolvedMerge(IChange<T> sourceA, IChange<T> sourceB, string message)
        {
            ChangeA = sourceA ?? throw new ArgumentNullException(nameof(sourceA));
            ChangeB = sourceB ?? throw new ArgumentNullException(nameof(sourceB));

            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public IChange<T> ChangeA { get; }
        public IChange<T> ChangeB { get; }

        public string Message { get; }
        bool IResolved<T>.Success => false;
    }
}
