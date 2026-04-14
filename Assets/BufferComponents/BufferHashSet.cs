using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BufferHashSet
{
    internal static HashSet<object> buffer = null;

    /// <summary>
    /// Creates a structure for accessing the buffer
    /// </summary>
    /// <typeparam name="TKey">Key type of the dictionary</typeparam>
    /// <typeparam name="TValue">Value type pf the dictionary</typeparam>
    /// <param name="minCapacity">The minimum capacity</param>
    /// <returns></returns>
    public static BufferHashSet<T> Get<T>(int minCapacity)
    {
        return new BufferHashSet<T>(minCapacity);
    }
    /// <summary>
    /// Empties the buffer of data
    /// </summary>
    public static void Clear()
    {
        buffer?.Clear();
    }
    /// <summary>
    /// Deletes the buffer, sending it to GC
    /// </summary>
    public static void Dispose()
    {
        buffer = null;
    }
}

public struct BufferHashSet<T> : IEnumerable<T>, IEnumerable
{
    public BufferHashSet(int minCapacity)
    {
        if (BufferHashSet.buffer == null)
            BufferHashSet.buffer = new HashSet<object>(minCapacity);
        else
            BufferHashSet.buffer.EnsureCapacity(minCapacity);

        BufferHashSet.buffer.Clear();
    }

    public readonly bool Add(T item) => BufferHashSet.buffer.Add(item);
    public readonly void Clear() => BufferHashSet.Clear();
    public readonly bool Contains(T item) => BufferHashSet.buffer.Contains(item);
    public readonly void CopyTo(T[] array) => CopyTo(array, 0);
    public readonly void CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex, BufferHashSet.buffer.Count);
    public readonly void CopyTo(T[] array, int arrayIndex, int count)
    {
        foreach(var obj in BufferHashSet.buffer)
        {   
            // End operations if an out or range error would occur or the count is done.
            if (count <= 0 || arrayIndex >= array.Length)
                return;

            array[arrayIndex] = (T)obj;
            arrayIndex++;
            count--;
        }
    }
    public readonly void ExceptWith(IEnumerable<T> other) => BufferHashSet.buffer.ExceptWith((IEnumerable<object>)other);
    public readonly void IntersectWith(IEnumerable<T> other) => BufferHashSet.buffer.IntersectWith((IEnumerable<object>)other);
    public readonly bool IsProperSubsetOf(IEnumerable<T> other) => BufferHashSet.buffer.IsProperSubsetOf((IEnumerable<object>)other);
    public readonly bool IsProperSupersetOf(IEnumerable<T> other) => BufferHashSet.buffer.IsProperSupersetOf((IEnumerable<object>)other);
    public readonly bool IsSubsetOf(IEnumerable<T> other) => BufferHashSet.buffer.IsSubsetOf((IEnumerable<object>)other);
    public readonly bool IsSupersetOf(IEnumerable<T> other) => BufferHashSet.buffer.IsSupersetOf((IEnumerable<object>)other);
    public readonly bool Overlaps(IEnumerable<T> other) => BufferHashSet.buffer.Overlaps((IEnumerable<object>)other);
    public readonly bool Remove(T item) => BufferHashSet.buffer.Remove(item);
    public readonly int RemoveWhere(Predicate<T> match)
    {
        return BufferHashSet.buffer.RemoveWhere(Predicate);
        bool Predicate(object item) => match((T)item);
    }
    public readonly bool SetEquals(IEnumerable<T> other) => BufferHashSet.buffer.SetEquals((IEnumerable<object>)other);
    public readonly void SymmetricExceptWith(IEnumerable<T> other) => BufferHashSet.buffer.SymmetricExceptWith((IEnumerable<object>)other);
    public readonly bool TryGetValue(T equalValue, out T actualValue)
    {
        bool value = BufferHashSet.buffer.TryGetValue(equalValue, out object actVal);
        
        if (value)
        {
            actualValue = (T)actVal;
            return true;
        }
        else
        {
            actualValue = default;
            return false;
        }
    }
    public readonly void UnionWith(IEnumerable<T> other) => BufferHashSet.buffer.UnionWith((IEnumerable<object>)other);

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        HashSet<object>.Enumerator enumerator;

        public Enumerator(HashSet<object> buffer)
        {
            enumerator = buffer.GetEnumerator();
        }

        public T Current => (T)enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            enumerator.Dispose();
            enumerator = default;
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            ((IEnumerator)enumerator).Reset();
        }
    }
}
