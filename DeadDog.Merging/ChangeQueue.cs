using System.Collections.Generic;

namespace DeadDog.Merging
{
    public class ChangeQueue<T> : IEnumerable<IChange<T>>
    {
        private List<IChange<T>> actions;

        private static int ancestorPositionSort(IChange<T> a, IChange<T> b)
        {
            return a.OldRange.Start.CompareTo(b.OldRange.Start);
        }

        public ChangeQueue(params IEnumerable<IChange<T>>[] collections)
        {
            actions = new List<IChange<T>>();

            foreach (var c in collections)
                actions.AddRange(c);

            actions.Sort(ancestorPositionSort);
        }

        public int Count
        {
            get { return actions.Count; }
        }

        public IChange<T> this[int index]
        {
            get { return actions[index]; }
        }

        IEnumerator<IChange<T>> IEnumerable<IChange<T>>.GetEnumerator()
        {
            foreach (var c in actions)
                yield return c;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var c in actions)
                yield return c;
        }
    }
}
