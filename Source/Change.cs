using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    public class Change<T>
    {
        private ChangeType type;
        private T value;
        private int pos;
        private Range range;

        public Change(ChangeType type, T value, int pos, Range range)
        {
            this.type = type;
            this.value = value;
            this.pos = pos;
            this.range = range;
        }

        public ChangeType ChangeType
        {
            get { return type; }
        }
        public T Value
        {
            get { return value; }
        }
        public int Position
        {
            get { return pos; }
        }
        public Range Range
        {
            get { return range; }
            set { range = value; }
        }

        public override string ToString()
        {
            string format = base.ToString();

            switch(type)
            {
                case ChangeType.Deletion: format = "Delete(\"{0}\", {2}, {1})"; break;
                case ChangeType.Insertion: format = "Insert(\"{0}\", {1}, {2})"; break;
            }

            return string.Format(format, value, pos, range);
        }
    }
}
