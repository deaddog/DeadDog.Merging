using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public interface IMoveIdentifier<T>
    {
        double? MoveWeight(Delete<T> delete, Insert<T> insert);
    }
}
