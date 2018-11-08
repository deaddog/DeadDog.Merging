using System;

namespace DeadDog.Merging
{
    public class Conflict<T>
    {
        public Conflict(IChange<T> changeA, IChange<T> changeB, string message)
        {
            ChangeA = changeA ?? throw new ArgumentNullException(nameof(changeA));
            ChangeB = changeB ?? throw new ArgumentNullException(nameof(changeB));

            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public IChange<T> ChangeA { get; }
        public IChange<T> ChangeB { get; }

        public string Message { get; }
    }
}
