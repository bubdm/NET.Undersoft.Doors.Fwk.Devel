using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Doors
{
    public static class ExtendArrayList
    {
        public static ArrayList Resize(this ArrayList array, int size)
        {
            int resize = size - array.Count;
            ArrayList fill = ArrayList.Repeat(null, resize);
            (array).Capacity = size;
            (array).AddRange(fill);
            return array;
        }
    }

    public static class ExtendList
    {
        public static List<T> Resize<T>(this List<T> array, int size)
        {
            (array).Capacity = size;
            (array).AddRange(new T[size - array.Count]);
            return array;
        }

        public static IList<T> PutRange<T>(this IList<T> array, IList<T> input)
        {
            return array.Union(input).ToList();
        }
        public static IList<T> PutRange<T>(this IList<T> array, T input, IList<T> output)
        {
             array.Add(input);
            return output;
        }
        public static T Put<T>(this IList<T> array, T input, T output)
        {
            array.Add(input);
            return output;
        }

    }
}
