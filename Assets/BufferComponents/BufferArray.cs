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
    /// Expands the buffer large enough to store capacity & returns a segment of it
    /// </summary>
    /// <param name="minCapacity"></param>
    /// <returns></returns>
    public static BufferArray<T> Get<T>(int minCapacity)
    {
        return new BufferArray<T>(minCapacity);
    }
    /// <summary>
    /// Empties the buffer of all existing values.
    /// </summary>
    public static void Clear()
    {   // Buffer not initialized, don't worry about it
        if (buffer == null)
            return;

        // Clear all entries in the buffer
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = default;
    }

    /// <summary>
    /// Empties a fixed number of entries from the buffer.
    /// </summary>
    /// <param name="maxCapacity"></param>
    public static void Clear(int maxCapacity)
    {   // Buffer not initialized ,don't worry about it
        if (buffer == null)
            return;

        for (int i = 0; i < buffer.Length && i < maxCapacity; i++)
            buffer[i] = default;
    }

    public static void Clear(ArraySegment<object> bufferSegment)
    {   // Not our buffer >:( Cannot be cleaned
        if (bufferSegment.Array != buffer)
            throw new InvalidOperationException("Buffer is not the originating buffer");

        // Buffer unititialized
        if (buffer == null)
            return;

        // Clear that specific segment of the buffer
        for (int i = bufferSegment.Offset; i < bufferSegment.Count; i++)
            buffer[i] = default;
    }

    /// <summary>
    /// Deletes the buffer, sending it to GC
    /// </summary>
    public static void Dispose()
    {
        buffer = null;
    }

}

/// <summary>
/// An array of items, allocated from a single global buffer
/// </summary>
/// <typeparam name="T"></typeparam>
public struct BufferArray<T> : IEnumerable<T>, IEnumerable
{
    #region Static Functions
    /// <summary>
    /// Expands the buffer large enough to store capacity & returns a segment of it
    /// </summary>
    /// <param name="minCapacity"></param>
    /// <returns></returns>
    public static BufferArray<T> Get(int minCapacity) => BufferArray.Get<T>(minCapacity);

    /// <summary>
    /// Empties a fixed number of entries from the buffer.
    /// </summary>
    /// <param name="maxCapacity"></param>
    public static void Clear(int maxCapacity) => BufferArray.Clear(maxCapacity);

    public static void Clear(BufferArray<T> bufferSegment) => BufferArray.Clear(bufferSegment.buffer);

    public static void Dispose() => BufferArray.Dispose();
    #endregion

    /// <summary>
    /// The internal object buffer
    /// </summary>
    ArraySegment<object> buffer;
    /// <summary>
    /// Accessor for buffer elements
    /// </summary>
    /// <param name="index">The index to get</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Buffer is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Index is out of range</exception>
    public T this[int index]
    {
        readonly get
        {
            // Throw relevant exceptions when directly indexing
            if (buffer == null)
                throw new NullReferenceException("Buffer is null");

            if (index < 0 || index >= buffer.Count)
                throw new ArgumentOutOfRangeException();

            return (T)buffer[index];
        }
        set
        {
            BufferArray<T> b = new BufferArray<T>(1);

            // Throw relevant exceptions when directly indexing
            if (buffer == null)
                throw new NullReferenceException("Buffer is null");

            if (index >= buffer.Count)
                throw new ArgumentOutOfRangeException();

            buffer[index] = value;
        }
    }
    /// <summary>
    /// Is the buffer valid
    /// </summary>
    public readonly bool IsValid => buffer != null;
    /// <summary>
    /// The length of this buffer array
    /// </summary>
    public readonly int Length => buffer.Count;

    public BufferArray(int minCapacity)
    {
        if (BufferArray.buffer == null || BufferArray.buffer.Length < minCapacity)
            BufferArray.buffer = new object[minCapacity];

        buffer = new ArraySegment<object>(BufferArray.buffer, 0, minCapacity);
    }

    /// <summary>
    /// Clears the buffer for this buffer array
    /// </summary>
    public void Clear()
    {
        Clear(this);
    }
    /// <summary>
    /// Invalidates this buffer array
    /// </summary>
    public void Invalidate()
    {
        buffer = null;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(buffer);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        /// <summary>
        /// The internal object buffer
        /// </summary>
        internal ArraySegment<object> buffer;
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
                if (buffer == null || index >= buffer.Count)
                    return default;

                return (T)buffer[index];
            }
        }

        object IEnumerator.Current => Current;

        public Enumerator(ArraySegment<object> buffer)
        {
            this.buffer = buffer;
            this.index = 0;
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
