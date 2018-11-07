namespace DeadDog.Merging
{
    public interface IResolved<T>
    {
        bool Success { get; }
    }

    public static class Resolved
    {
        public static IResolved<T> AsMerge<T>(IChange<T> sourceA, IChange<T> sourceB, IChange<T> merged)
        {
            return new ResolvedMerge<T>
            (
                sourceA: sourceA,
                sourceB: sourceB,
                merged: merged
            );
        }
        public static IResolved<T> AsConflict<T>(IChange<T> sourceA, IChange<T> sourceB, string message)
        {
            return new UnResolvedMerge<T>
            (
                sourceA: sourceA,
                sourceB: sourceB,
                message: message
            );
        }
        public static IResolved<T> AsNoMerge<T>(IChange<T> sourceA, IChange<T> sourceB)
        {
            return new ResolvedNoMerge<T>
            (
                sourceA: sourceA,
                sourceB: sourceB
            );
        }
    }
}
