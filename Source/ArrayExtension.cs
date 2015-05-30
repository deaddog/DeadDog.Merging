using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    internal static class ArrayExtension
    {
        public static TArray[] GetRange<TArray>(this TArray[] array, int index)
        {
            return GetRange(array, Range.FromStartLength(index, array.Length - index));
        }
        public static TArray[] GetRange<TArray>(this TArray[] array, Range range)
        {
            TArray[] arr = new TArray[range.Length];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = array[i + range.Start];
            return arr;
        }

        public static TArray[] GetRange<TArray>(this TArray[] array, Range range1, int index2)
        {
            return GetRange(array, range1, new Range(index2, array.Length - index2));
        }
        public static TArray[] GetRange<TArray>(this TArray[] array, Range range1, Range range2)
        {
            TArray[] arr = new TArray[range1.Length + range2.Length];
            for (int i = 0; i < range1.Length; i++)
                arr[i] = array[i + range1.Start];
            for (int i = 0; i < range2.Length; i++)
                arr[i + range1.Length] = array[i + range2.Start];
            return arr;
        }
        public static TArray[] GetRange<TArray>(this TArray[] array, Range range1, TArray[] middle, int index2)
        {
            return GetRange(array, range1, middle, new Range(index2, array.Length - index2));
        }
        public static TArray[] GetRange<TArray>(this TArray[] array, Range range1, TArray[] middle, Range range2)
        {
            TArray[] arr = new TArray[range1.Length + range2.Length + middle.Length];
            for (int i = 0; i < range1.Length; i++)
                arr[i] = array[i + range1.Start];

            middle.CopyTo(arr, range1.Length);

            for (int i = 0; i < range2.Length; i++)
                arr[i + range1.Length + middle.Length] = array[i + range2.Start];
            return arr;
        }
    }
}
