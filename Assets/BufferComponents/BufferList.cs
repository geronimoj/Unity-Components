using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BufferList
{
    internal static List<object> buffer = null;

    /// <summary>
    /// Empties the buffer
    /// </summary>
    public static void Clear()
    {
        buffer?.Clear();
    }
    /// <summary>
    /// Deletes the buffer, releasing it to GC
    /// </summary>
    public static void Dispose()
    {
        buffer = null;
    }
}

public static class BufferList<T>
{
    public static Buffer Get(int minCapacity)
    {
        // Allocate the buffer if necessary
        if (BufferList.buffer == null)
            BufferList.buffer = new List<object>(minCapacity);
        else if (BufferList.buffer.Capacity < minCapacity)
        {
            BufferList.buffer.Capacity = minCapacity;
        }

        // Return a new buffer. This automatically points to the buffer
        return new Buffer();
    }
    /// <summary>
    /// Empties the buffer
    /// </summary>
    public static void Clear() => BufferList.Clear();
    /// <summary>
    /// Deletes the buffer, releasing it to GC
    /// </summary>
    public static void Dispose() => BufferList.Dispose();

    public struct Buffer : IEnumerable<T>, IEnumerable
    {
        public readonly bool IsValid => BufferList.buffer != null;

        public readonly List<object> List => BufferList.buffer;

        public readonly T this[int index]
        {
            get
            {
                // Throw relevant exceptions when directly indexing
                if (BufferList.buffer == null)
                    throw new NullReferenceException("Buffer is null");

                if (index >= BufferList.buffer.Count)
                    throw new ArgumentOutOfRangeException();

                return (T)BufferList.buffer[index];
            }
            set
            {   // Throw relevant exceptions when directly indexing
                if (BufferList.buffer == null)
                    throw new NullReferenceException("Buffer is null");

                BufferList.buffer[index] = value;
            }
        }

        public readonly int Count => BufferList.buffer?.Count ?? 0;

        public readonly int Capacity
        {
            get => BufferList.buffer?.Capacity ?? 0;
            set
            {
                // Allow expanding of buffer but never allow reduction.
                if (value > Capacity)
                {
                    // Create a new buffer if we need one
                    BufferList.buffer ??= new List<object>(value);
                    BufferList.buffer.Capacity = value;
                }
            }
        }

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator();
        }

        public readonly IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }

        /* Functions to implement
        ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList

        public void Add(T item);
        public void AddRange(IEnumerable<T> collection);
        public ReadOnlyCollection<T> AsReadOnly();
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer);
        public int BinarySearch(T item);
        public int BinarySearch(T item, IComparer<T> comparer);
        public void Clear();
        public bool Contains(T item);
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter);
        public void CopyTo(T[] array, int arrayIndex);
        public void CopyTo(T[] array);
        public void CopyTo(int index, T[] array, int arrayIndex, int count);
        public bool Exists(Predicate<T> match);
        public T Find(Predicate<T> match);
        public List<T> FindAll(Predicate<T> match);
        public int FindIndex(int startIndex, int count, Predicate<T> match);
        public int FindIndex(int startIndex, Predicate<T> match);
        public int FindIndex(Predicate<T> match);
        public T FindLast(Predicate<T> match);
        public int FindLastIndex(int startIndex, int count, Predicate<T> match);
        public int FindLastIndex(int startIndex, Predicate<T> match);
        public int FindLastIndex(Predicate<T> match);
        public void ForEach(Action<T> action);
        public List<T> GetRange(int index, int count);
        public int IndexOf(T item, int index, int count);
        public int IndexOf(T item, int index);
        public int IndexOf(T item);
        public void Insert(int index, T item);
        public void InsertRange(int index, IEnumerable<T> collection);
        public int LastIndexOf(T item);
        public int LastIndexOf(T item, int index);
        public int LastIndexOf(T item, int index, int count);
        public bool Remove(T item);
        public int RemoveAll(Predicate<T> match);
        public void RemoveAt(int index);
        public void RemoveRange(int index, int count);
        public void Reverse(int index, int count);
        public void Reverse();
        public void Sort(Comparison<T> comparison);
        public void Sort(int index, int count, IComparer<T> comparer);
        public void Sort();
        public void Sort(IComparer<T> comparer);
        public T[] ToArray();
        public void TrimExcess();
        public bool TrueForAll(Predicate<T> match);
         */

        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The index the enumerator is currently at
            /// </summary>
            int index;

            /// <summary>
            /// The current object the enumeartor is targeting
            /// </summary>
            public T Current
            {
                get
                {   // If invalid buffer or buffer is out of range
                    if (BufferList.buffer == null || index >= BufferList.buffer.Count)
                        return default;

                    return (T)BufferList.buffer[index];
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                index = -1;
            }

            public bool MoveNext()
            {   // Buffer is empty
                if (BufferList.buffer == null ||
                    index == -1)
                    return false;

                index++;
                return index < BufferList.buffer.Count;
            }

            public void Reset()
            {
                index = 0;
            }

        }
    }
}
