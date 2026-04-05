using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolHashSet<T> : HashSet<T>
{
    /// <summary>
    /// The pool of available hashset
    /// </summary>
    static Stack<PoolHashSet<T>> pool = null;

    public PoolHashSet(int capacity) : base(capacity)
    {
    }

    public PoolHashSet(IEnumerable<T> collection) : base(collection)
    {
    }

    /// <summary>
    /// Gets a pooled hashset from the pool
    /// </summary>
    /// <returns></returns>
    public static PoolHashSet<T> Get()
    {
        return Get(1);
    }
    /// <summary>
    /// Gets a pooled hashset from the pool
    /// </summary>
    /// <param name="minCapacity">The minimum capacity of the hashset</param>
    /// <returns></returns>
    public static PoolHashSet<T> Get(int minCapacity)
    {
        pool ??= new Stack<PoolHashSet<T>>();

        if (pool.Count == 0)
        {
            return new PoolHashSet<T>(minCapacity);
        }

        // Make sure the capacity meets the minimum capacity
        var hashset = pool.Peek();
        hashset.EnsureCapacity(minCapacity);

        // People probably don't want a hashset with stuff already in it :P
        hashset.Clear();
        return hashset;
    }

    /// <summary>
    /// Releases this hashset back to the pool
    /// </summary>
    public void Release()
    {
        // Make sure the stack exists. This way you can dynamically create pool hashsets without going through the Get() function.
        // Don't know why you would want to do that, but just in case, Release() will put it in the pool.
        pool ??= new Stack<PoolHashSet<T>>();
        pool.Push(this);
    }

    /// <summary>
    /// Deletes all not-in-use pooled hashsets
    /// </summary>
    public static void DisposeAll()
    {
        pool.Clear();
    }
}
