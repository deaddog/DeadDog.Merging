using System.Collections.Generic;
using System.Linq;

namespace DeadDog.Merging
{
    internal abstract class ArrayOperation<T>
    {
        public static ArrayOperation<T> operator +(ArrayOperation<T> a, ArrayOperation<T> b)
        {
            return new conOp<T>(a, b);
        }
        public static ArrayOperation<T> operator +(ArrayOperation<T> a, T[] array)
        {
            return a + new rangeOp<T>(array, Range.FromStartLength(0, array.Length));
        }


        public static implicit operator T[](ArrayOperation<T> operation)
        {
            T[] array = new T[operation.Length];
            operation.CopyTo(array, 0);
            return array;
        }

        public abstract int Length { get; }
        public abstract void CopyTo(T[] array, int index);

        public class rangeOp<T2> : ArrayOperation<T2>
        {
            public readonly T2[] array;
            private Range range;

            public rangeOp(T2[] array, Range range)
            {
                this.array = array;
                this.range = range;
            }

            public override int Length
            {
                get { return range.Length; }
            }
            public override void CopyTo(T2[] array, int index)
            {
                if (range.Length == this.array.Length)
                    this.array.CopyTo(array, index);
                else
                {
                    for (int i = 0; i < range.Length; i++)
                        array[i + index] = this.array[i + range.Start];
                }
            }
        }

        public class conOp<T2> : ArrayOperation<T2>
        {
            private List<ArrayOperation<T2>> operations;

            public conOp(ArrayOperation<T2> a, ArrayOperation<T2> b)
            {
                this.operations = new List<ArrayOperation<T2>>();

                if (a is conOp<T2>)
                    this.operations.AddRange((a as conOp<T2>).operations);
                else
                    this.operations.Add(a);

                if (b is conOp<T2>)
                    this.operations.AddRange((b as conOp<T2>).operations);
                else
                    this.operations.Add(b);
            }

            public override int Length
            {
                get { return operations.Sum(x => x.Length); }
            }
            public override void CopyTo(T2[] array, int index)
            {
                for (int i = 0; i < operations.Count; i++)
                {
                    operations[i].CopyTo(array, index);
                    index += operations[i].Length;
                }
            }
        }
    }
}
