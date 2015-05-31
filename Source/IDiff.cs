using System;
using System.Collections.Generic;

namespace DeadDog.Merging
{
    public interface IDiff<T> where T : IEquatable<T>
    {
        IEnumerable<IChange<T>> Diff(IEnumerable<T> origin, IEnumerable<T> modified);
    }
}
