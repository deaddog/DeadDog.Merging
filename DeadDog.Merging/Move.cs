using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public class Move<T> : IChange<T>
    {
        public Move(Delete<T> from, Insert<T> to, bool first)
        {
            From = from ?? throw new System.ArgumentNullException(nameof(from));
            To = to ?? throw new System.ArgumentNullException(nameof(to));

            First = first;
        }

        IImmutableList<T> IChange<T>.Value => From.Value;

        Range IChange<T>.OldRange => From.OldRange;
        Range IChange<T>.NewRange => To.NewRange;

        public Delete<T> From { get; }
        public Insert<T> To { get; }

        public bool First { get; }
    }
}
