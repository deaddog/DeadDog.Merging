using System;
using System.Collections.Immutable;

namespace DeadDog.Merging
{
    public class ConflictsException<T> : Exception
    {
        public ConflictsException(IImmutableList<Conflict<T>> conflicts)
            : base(message: $"Conflicts were found, see {nameof(Conflicts)} for details.")
        {
            Conflicts = conflicts ?? throw new ArgumentNullException(nameof(conflicts));
        }

        public IImmutableList<Conflict<T>> Conflicts { get; }
    }
}
