using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadDog.Merging
{
    internal static class ArrayExtension
    {
        public static TArray[] GetRange<TArray>(this TArray[] array, Range range)
        {
            TArray[] arr = new TArray[range.Length];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = array[i + range.Start];
            return arr;
        }
    }
}
