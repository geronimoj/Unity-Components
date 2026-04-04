using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolList<T> : List<T>
{
    /// <summary>
    /// The pool of available lists
    /// </summary>
    static Stack<PoolList<T>> pool = null;

    public PoolList(int capacity) : base(capacity)
    {
    }

    public PoolList(IEnumerable<T> collection) : base (collection)
    {
    }

    /// <summary>
    /// Gets a pooled list from the pool
    /// </summary>
    /// <returns></returns>
    public static PoolList<T> Get()
    {
        return Get(1);
    }
    /// <summary>
    /// Gets a pooled list from the pool
    /// </summary>
    /// <param name="minCapacity">The minimum capacity of the list</param>
    /// <returns></returns>
    public static PoolList<T> Get(int minCapacity)
    {
        pool ??= new Stack<PoolList<T>>();

        if (pool.Count == 0)
        {
            return new PoolList<T>(minCapacity);
        }

        var list = pool.Peek();

        // Make sure the capacity meets the minimum capacity
        if (list.Capacity < minCapacity)
            list.Capacity = minCapacity;

        // People probably don't want a list with stuff already in it :P
        list.Clear();
        return list;
    }

    /// <summary>
    /// Releases this list back to the pool
    /// </summary>
    public void Release()
    {
        // Make sure the stack exists. This way you can dynamically create pool lists without going through the Get() function.
        // Don't know why you would want to do that, but just in case, Release() will put it in the pool.
        pool ??= new Stack<PoolList<T>>();
        pool.Push(this);
    }

    /// <summary>
    /// Deletes all not-in-use pooled lists
    /// </summary>
    public static void DisposeAll()
    {
        pool.Clear();
    }
}
