using System.Collections.ObjectModel;
using System.Linq;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();

        public static IList<T> ToList<T>(this IEnumerable<T> source, int initialCapacity)
        {
            var list = new List<T>(initialCapacity);
            list.AddRange(source);
            return list;
        }

        public static bool TryMoveNext<T>(this IEnumerator<T> source, out T current)
        {
            if (source.MoveNext())
            {
                current = source.Current;
                return true;
            }
            else
            {
                current = default!;
                return false;
            }
        }

        public static T[] ToArray<T>(this IEnumerable<T> source, int sizeHint)
        {
            var array = new T[sizeHint];
            int index = 0;
            foreach (var item in source)
            {
                array[index] = item;
                index++;
            }
            return array;
        }

        public static bool TryFirst<T>(this IEnumerable<T> source, out T element)
        {
            using var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                element = enumerator.Current;
                return true;
            }

            element = default!;
            return false;
        }

        public static bool TrySingle<T>(this IEnumerable<T> source, out T element)
        {
            using var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                element = enumerator.Current;
                if (enumerator.MoveNext())
                {
                    element = default!;
                    return false;
                }
                return true;
            }

            element = default!;
            return false;
        }

        public static bool TryLast<T>(this IEnumerable<T> source, out T element)
        {
            element = default!;
            var result = false;

            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                element = enumerator.Current;
                result = true;
            }

            return result;
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary
        ) => dictionary switch
        {
            IReadOnlyDictionary<TKey, TValue> readOnly => readOnly,
            _ => new ReadOnlyDictionary<TKey, TValue>(dictionary)
        };

        public static SortedDictionary<TKey, TValue> AsSorted<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary
        ) => dictionary switch
        {
            SortedDictionary<TKey, TValue> readOnly => readOnly,
            _ => new SortedDictionary<TKey, TValue>(dictionary)
        };
    }
}
