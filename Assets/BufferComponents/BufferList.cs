using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BufferList
{
    internal static List<object> buffer = null;
    /// <summary>
    /// Creates a buffer object for handling a specific type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="minCapacity"></param>
    /// <returns></returns>
    public static BufferList<T> Get<T>(int minCapacity)
    {
        return new BufferList<T>(minCapacity);
    }
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

public struct BufferList<T> : IEnumerable<T>, IEnumerable
{
    #region Static Functions
    public static BufferList<T> Get(int minCapacity)
    {
        return BufferList.Get<T>(minCapacity);
    }
    /// <summary>
    /// Deletes the buffer, releasing it to GC
    /// </summary>
    public static void Dispose() => BufferList.Dispose();
    #endregion

    public BufferList(int minCapacity)
    {
        // Allocate the buffer if necessary
        if (BufferList.buffer == null)
            BufferList.buffer = new List<object>(minCapacity);
        else if (BufferList.buffer.Capacity < minCapacity)
        {
            BufferList.buffer.Capacity = minCapacity;
        }
    }

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

    public readonly void Add(T item) => BufferList.buffer.Add(item);
    public readonly void AddRange(IEnumerable<T> collection) => BufferList.buffer.AddRange((IEnumerable<object>)collection);

    public readonly bool Contains(T item) => BufferList.buffer.Contains(item);

    public readonly void CopyTo(T[] array) => CopyTo(array, 0);
    public readonly void CopyTo(T[] array, int arrayIndex) => CopyTo(0, array, arrayIndex, BufferList.buffer.Count);
    public readonly void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        // Starting at index, write to arrayIndex for count times into the output array
        for (; arrayIndex < array.Length && count > 0;)
        {
            array[arrayIndex] = (T)BufferList.buffer[index];

            // Increment indexes & count
            arrayIndex++;
            index++;
            count--;
        }
    }

    /// <summary>
    /// Empties the buffer
    /// </summary>
    public readonly void Clear() => BufferList.Clear();

    public readonly bool Exists(Predicate<T> match) => FindIndex(match) != -1;
    public readonly T Find(Predicate<T> match)
    {
        int bufferCount = BufferList.buffer.Count;
        for (int index = 0; index < bufferCount; index++)
        {
            T obj = (T)BufferList.buffer[index];
            if (match(obj))
                return obj;

            index++;
        }

        return default;
    }
    public readonly List<T> FindAll(Predicate<T> match)
    {
        int bufferCount = BufferList.buffer.Count;
        List<T> ret = new List<T>(bufferCount); // <- Probably better to assume capacity is at max to avoid GC alloc on expansions

        for (int index = 0; index < bufferCount; index++)
        {
            T obj = (T)BufferList.buffer[index];
            if (match(obj))
                ret.Add(obj);

            index++;
        }

        return ret;
    }
    public readonly int FindIndex(Predicate<T> match) => FindIndex(0, match);
    public readonly int FindIndex(int index, Predicate<T> match) => FindIndex(index, BufferList.buffer.Count, match);
    public readonly int FindIndex(int index, int count, Predicate<T> match)
    {
        int bufferCount = BufferList.buffer.Count;
        for (; index < bufferCount && count > 0;)
        {
            if (match((T)BufferList.buffer[index]))
                return index;

            index++;
            count--;
        }

        return -1;
    }
    public readonly int IndexOf(T item) => BufferList.buffer.IndexOf(item);
    public readonly int IndexOf(T item, int index) => BufferList.buffer.IndexOf(item, index);
    public readonly int IndexOf(T item, int index, int count) => BufferList.buffer.IndexOf(item, index, count);
    public readonly int LastIndexOf(T item) => BufferList.buffer.LastIndexOf(item);
    public readonly int LastIndexOf(T item, int index) => BufferList.buffer.LastIndexOf(item, index);
    public readonly int LastIndexOf(T item, int index, int count) => BufferList.buffer.LastIndexOf(item, index, count);

    public readonly void Insert(int index, T item) => BufferList.buffer.Insert(index, item);
    public readonly void InsertRange(int index, IEnumerable<T> collection) => BufferList.buffer.InsertRange(index, (IEnumerable<object>)collection);

    public readonly void ForEach(Action<T> action)
    {
        foreach (var item in BufferList.buffer)
            action((T)item);
    }
    /// <summary>
    /// Creates a new list containing elements from the buffer
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public readonly List<T> GetRange(int index, int count)
    {
        // Return a new list
        List<T> ret = new List<T>(count);

        int bufferCount = BufferList.buffer.Count;
        int retCount = 0;

        for (; index < bufferCount && retCount < count; index++)
        {
            ret.Add((T)BufferList.buffer[index]);
            retCount++; // <- Probably more efficient that calling .Count constantly
        }

        return ret;
    }

    public readonly bool Remove(T item) => BufferList.buffer.Remove(item);
    public readonly int RemoveAll(Predicate<T> match)
    {
        int count = 0;
        for (int i = 0; i < BufferList.buffer.Count; i++)
        {
            if (match((T)BufferList.buffer[i]))
            {
                RemoveAt(i);
                count++;
                i--;
            }
        }

        return count;
    }
    public readonly void RemoveAt(int index) => BufferList.buffer.RemoveAt(index);
    public readonly void RemoveRange(int index, int count) => BufferList.buffer.RemoveRange(index, count);
    public readonly void Reverse() => BufferList.buffer.Reverse();
    public readonly void Reverse(int index, int count) => BufferList.buffer.Reverse(index, count);

    public readonly T[] ToArray()
    {
        int count = BufferList.buffer.Count;
        T[] ret = new T[count];

        for (int i = 0; i < count; i++)
            ret[i] = (T)BufferList.buffer[i];

        return ret;
    }

    public readonly List<T> ToList()
    {
        int count = BufferList.buffer.Count;
        List<T> ret = new List<T>(count);

        for (int i = 0; i < count; i++)
            ret.Add((T)BufferList.buffer[i]);

        return ret;
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
    ICollection<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList

    public ReadOnlyCollection<T> AsReadOnly();
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer);
    public int BinarySearch(T item);
    public int BinarySearch(T item, IComparer<T> comparer);
    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter);
    public T FindLast(Predicate<T> match);
    public int FindLastIndex(int startIndex, int count, Predicate<T> match);
    public int FindLastIndex(int startIndex, Predicate<T> match);
    public int FindLastIndex(Predicate<T> match);
    public void Sort(Comparison<T> comparison);
    public void Sort(int index, int count, IComparer<T> comparer);
    public void Sort();
    public void Sort(IComparer<T> comparer);
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
