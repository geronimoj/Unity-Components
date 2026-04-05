using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    /// <summary>
    /// The pool of available dictionaries
    /// </summary>
    static Stack<PoolDictionary<TKey, TValue>> pool = null;

    public PoolDictionary(int minCapacity) : base (minCapacity)
    {
    }

    /// <summary>
    /// Gets a pooled dictionaries from the pool
    /// </summary>
    /// <returns></returns>
    public static PoolDictionary<TKey, TValue> Get()
    {
        return Get(1);
    }
    /// <summary>
    /// Gets a pooled dictionaries from the pool
    /// </summary>
    /// <param name="minCapacity">The minimum capacity of the dictionaries</param>
    /// <returns></returns>
    public static PoolDictionary<TKey, TValue> Get(int minCapacity)
    {
        pool ??= new Stack<PoolDictionary<TKey, TValue>>();

        if (pool.Count == 0)
        {
            return new PoolDictionary<TKey, TValue>(minCapacity);
        }

        var dictionary = pool.Peek();
        dictionary.EnsureCapacity(minCapacity);
        // People probably don't want a dictionaries with stuff already in it :P
        dictionary.Clear();
        return dictionary;
    }

    /// <summary>
    /// Releases this dictionaries back to the pool
    /// </summary>
    public void Release()
    {
        // Make sure the stack exists. This way you can dynamically create pool dictionaries without going through the Get() function.
        // Don't know why you would want to do that, but just in case, Release() will put it in the pool.
        pool ??= new Stack<PoolDictionary<TKey, TValue>>();
        pool.Push(this);
    }

    /// <summary>
    /// Deletes all not-in-use pooled dictionaries
    /// </summary>
    public static void DisposeAll()
    {
        pool.Clear();
    }
}
