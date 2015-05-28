using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public delegate IEnumerable<Change<T>> Diff<T>(IEnumerable<T> a, IEnumerable<T> b);
}
