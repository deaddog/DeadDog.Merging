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

        int IChange<T>.Position => To.Position;
        Range IChange<T>.Range => From.Range;

        public Delete<T> From { get; }
        public Insert<T> To { get; }

        public bool First { get; }

        public override string ToString() => $"Move(\"{From}\" -> \"{To}\")";
    }
}
