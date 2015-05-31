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

        public class rangeOp<T> : ArrayOperation<T>
        {
            public readonly T[] array;
            private Range range;

            public rangeOp(T[] array, Range range)
            {
                this.array = array;
                this.range = range;
            }

            public override int Length
            {
                get { return range.Length; }
            }
            public override void CopyTo(T[] array, int index)
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

        public class conOp<T> : ArrayOperation<T>
        {
            private List<ArrayOperation<T>> operations;

            public conOp(ArrayOperation<T> a, ArrayOperation<T> b)
            {
                this.operations = new List<ArrayOperation<T>>();

                if (a is conOp<T>)
                    this.operations.AddRange((a as conOp<T>).operations);
                else
                    this.operations.Add(a);

                if (b is conOp<T>)
                    this.operations.AddRange((b as conOp<T>).operations);
                else
                    this.operations.Add(b);
            }

            public override int Length
            {
                get { return operations.Sum(x => x.Length); }
            }
            public override void CopyTo(T[] array, int index)
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
