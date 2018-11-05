using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class ChangeQueue<T> : IEnumerable<IChange<T>>
    {
        private List<IChange<T>> actions;

        #region Initial sorting
        private static int ancestorPositionSort(IChange<T> a, IChange<T> b)
        {
            return ancestorPosition(a).CompareTo(ancestorPosition(b));
        }

        private static int ancestorPosition(IChange<T> change)
        {
            switch (change)
            {
                case Delete<T> delete: return delete.Range.Start;
                case Insert<T> insert: return insert.Position;
                case Move<T> move: return move.To.Position;

                default:
                    throw new ArgumentException($"Unknown change type: {change.GetType().Name}.");
            }
        }
        #endregion

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
