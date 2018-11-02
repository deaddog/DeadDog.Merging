using System;

namespace DeadDog.Merging
{
    internal static class ArrayExtension
    {
        public static ArrayOperation<T> Subarray<T>(this T[] array, int index)
        {
            return new ArrayOperation<T>.rangeOp<T>(array, Range.FromStartLength(index, array.Length - index));
        }
        public static ArrayOperation<T> Subarray<T>(this T[] array, int index, int length)
        {
            return new ArrayOperation<T>.rangeOp<T>(array, Range.FromStartLength(index, length));
        }
        public static ArrayOperation<T> Subarray<T>(this T[] array, Range range)
        {
            return new ArrayOperation<T>.rangeOp<T>(array, range);
        }
    }
}
