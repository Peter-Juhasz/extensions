using System.Linq;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list) => list switch
        {
            IReadOnlyList<T> readOnly => readOnly,
            _ => list.ToList().AsReadOnly()
        };

        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> list) => list switch
        {
            IReadOnlyCollection<T> readOnly => readOnly,
            _ => list.ToList().AsReadOnly()
        };

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> elements)
        {
            if (source is List<T> list)
            {
                list.AddRange(elements);
            }
            else
            {
                foreach (var item in elements)
                {
                    source.Add(item);
                }
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> elements)
        {
            foreach (var item in elements)
            {
                source.Add(item.Key, item.Value);
            }
        }

        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> elements)
        {
            if (source is List<T> list)
            {
                list.RemoveRange(elements);
            }
            else
            {
                foreach (var item in elements)
                {
                    source.Remove(item);
                }
            }
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate, int startIndex)
        {
            if (startIndex < 0 || startIndex >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            for (int i = startIndex; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate) => list.IndexOf(predicate, 0);

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item, int startIndex, IEqualityComparer<T> comparer)
        {
            if (startIndex < 0 || startIndex >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            for (int i = startIndex; i < list.Count; i++)
            {
                if (comparer.Equals(list[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, T item, int startIndex, IEqualityComparer<T> comparer)
        {
            if (startIndex < 0 || startIndex >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            for (int i = startIndex; i >= 0; i--)
            {
                if (comparer.Equals(list[i], item))
                {
                    return i;
                }
            }
            
            return -1;
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate, int startIndex)
        {
            if (startIndex < 0 || startIndex >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            for (int i = startIndex; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate) => list.IndexOf(predicate, list.Count - 1);

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item) => list.IndexOf(item, 0, EqualityComparer<T>.Default);

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, T item) => list.LastIndexOf(item, list.Count - 1, EqualityComparer<T>.Default);

        public static bool CanMoveUp<T>(this IList<T> list, T item) => list.IndexOf(item) > 0;

        public static bool CanMoveDown<T>(this IList<T> list, T item)
        {
            var index = list.IndexOf(item);
            if (index == -1) return false;
            return index < list.Count - 1;
        }

        public static void MoveUp<T>(this IList<T> list, T item)
        {
            var index = list.IndexOf(item);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(item));
            }
            if (index == 0)
            {
                throw new InvalidOperationException();
            }

            list.RemoveAt(index);
            list.Insert(index - 1, item);
        }

        public static void MoveDown<T>(this IList<T> list, T item)
        {
            var index = list.IndexOf(item);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(item));
            }
            if (index == list.Count - 1)
            {
                throw new InvalidOperationException();
            }

            list.RemoveAt(index);
            list.Insert(index + 1, item);
        }
    }
}
