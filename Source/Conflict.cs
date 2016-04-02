using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class Conflict<T>
    {
        private IChange<T> a, b;
        private string message;

        public Conflict(IChange<T> a, IChange<T> b, string message)
        {
            this.a = a;
            this.b = b;
            this.message = message;
        }

        public IChange<T> A
        {
            get { return a; }
        }
        public IChange<T> B
        {
            get { return b; }
        }

        public string Message
        {
            get { return message; }
        }
    }
}
