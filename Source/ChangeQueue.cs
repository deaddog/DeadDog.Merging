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
        private static int ancestorPositionSort<T>(IChange<T> a, IChange<T> b)
        {
            return ancestorPosition((dynamic)a).CompareTo(ancestorPosition((dynamic)b));
        }

        private static int ancestorPosition<T>(Delete<T> delete)
        {
            return delete.Range.Start;
        }
        private static int ancestorPosition<T>(Insert<T> delete)
        {
            return delete.Position;
        }
        private static int ancestorPosition<T>(Move<T> delete)
        {
            return delete.Position1;
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
