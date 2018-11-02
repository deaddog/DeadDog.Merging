using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public interface IChange<T>
    {
        IChange<T2> Clone<T2>(T2[] newValue);

        T[] Value
        {
            get;
        }
        int Position
        {
            get;
        }
        Range Range
        {
            get;
        }
    }
}
