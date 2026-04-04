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
    public static Buffer Get(int maxCapacity)
    {
        // Allocate the buffer if necessary
        if (BufferList.buffer == null)
            BufferList.buffer = new List<object>(maxCapacity);
        else if (BufferList.buffer.Capacity < maxCapacity)
        {
            BufferList.buffer.Capacity = maxCapacity;
        }

        // Just clear it just in case they need it that way
        BufferList.buffer.Clear();
        return new Buffer(BufferList.buffer);
    }
    /// <summary>
    /// Empties the buffer
    /// </summary>
    public static void Clear() => BufferList.Clear();
    /// <summary>
    /// Deletes the buffer, releasing it to GC
    /// </summary>
    public static void Dispose() => BufferList.Dispose();

    public struct Buffer : IEnumerator<T>
    {
        List<object> buffer;
        /// <summary>
        /// The index the enumerator is currently at
        /// </summary>
        int index;


        public T this[int index]
        {
            get
            {
                // Throw relevant exceptions when directly indexing
                if (buffer == null)
                    throw new NullReferenceException("Buffer is null");

                if (index >= buffer.Count)
                    throw new ArgumentOutOfRangeException();

                return (T)buffer[index];
            }
        }

        /// <summary>
        /// The current object the enumeartor is targeting
        /// </summary>
        public T Current
        {
            get
            {   // If invalid buffer or buffer is out of range
                if (buffer == null || index >= buffer.Count)
                    return default;

                return (T)buffer[index];
            }
        }

        object IEnumerator.Current => Current;

        public Buffer(List<object> buffer)
        {
            this.buffer = buffer;
            index = 0;
        }

        public void Dispose()
        {
            buffer = null;
            index = 0;
        }

        public bool MoveNext()
        {   // Buffer is empty
            if (buffer == null)
                return false;

            index++;
            return index < buffer.Count;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}
