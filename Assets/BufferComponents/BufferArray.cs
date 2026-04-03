using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We do it like this so there is only 1 buffer array at any given time. If we don't do it like this, each typed buffer array will have its own buffer
public static class BufferArray
{
    /// <summary>
    /// The buffer to use for the array
    /// </summary>
    internal static object[] buffer = null;

    /// <summary>
    /// For pre-emptively initializing the buffer with a minimum size.
    /// </summary>
    /// <param name="minCapacity"></param>
    public static void Initialize(int minCapacity)
    {
        if (buffer == null || buffer.Length < minCapacity)
            buffer = new object[minCapacity];
    }

    /// <summary>
    /// Deletes the buffer, sending it to GC
    /// </summary>
    public static void Dispose()
    {
        buffer = null;
    }

}

public static class BufferArray<T>
{
    /// <summary>
    /// Expands the buffer large enough to store capacity & returns a segment of it
    /// </summary>
    /// <param name="minCapacity"></param>
    /// <returns></returns>
    public static Buffer Get(int minCapacity)
    {
        if (BufferArray.buffer == null || BufferArray.buffer.Length < minCapacity)
            BufferArray.buffer = new object[minCapacity];

        return new Buffer(new ArraySegment<object>(BufferArray.buffer, 0, minCapacity));
    }

    /// <summary>
    /// Empties the buffer of all existing values.
    /// </summary>
    public static void Clear()
    {   // Buffer not initialized, don't worry about it
        if (BufferArray.buffer == null)
            return;

        // Clear all entries in the buffer
        for (int i = 0; i < BufferArray.buffer.Length; i++)
            BufferArray.buffer[i] = default;
    }
    
    /// <summary>
    /// Empties a fixed number of entries from the buffer.
    /// </summary>
    /// <param name="maxCapacity"></param>
    public static void Clear(int maxCapacity)
    {   // Buffer not initialized ,don't worry about it
        if (BufferArray.buffer == null)
            return;

        for (int i = 0; i < BufferArray.buffer.Length && i < maxCapacity; i++)
            BufferArray.buffer[i] = default;
    }

    public static void Clear(Buffer bufferSegment)
    {   // Not our buffer >:( Cannot be cleaned
        if (bufferSegment.buffer.Array != BufferArray.buffer)
            throw new InvalidOperationException("Buffer is not the originating buffer");

        // Buffer unititialized
        if (BufferArray.buffer == null)
            return;

        // Clear that specific segment of the buffer
        for (int i = bufferSegment.buffer.Offset; i < bufferSegment.buffer.Count; i++)
            BufferArray.buffer[i] = default;
    }

    public struct Buffer : IEnumerator<T>
    {
        /// <summary>
        /// The internal object buffer
        /// </summary>
        internal ArraySegment<object> buffer;
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

        public Buffer(ArraySegment<object> buffer)
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
