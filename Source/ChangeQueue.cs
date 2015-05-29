using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class ChangeQueue
    {
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
    }
}
