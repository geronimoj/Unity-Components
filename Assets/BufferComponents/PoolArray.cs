using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolArray<T>
{
    public static List<T[]> pool = null;

    /// <summary>
    /// Gets an array of the specified length from the pool
    /// </summary>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public static ArraySegment<T> Get(int capacity)
    {
        // Pool is empty or unitintialized, create a new array
        if (pool == null || pool.Count == 0)
            return new T[capacity];

        // Search for the first array in the pool that can fit the requested size.
        foreach(var array in pool)
        {
            if (array.Length >= capacity)
                return new ArraySegment<T>(array, 0, capacity);
        }

        // Failed to find an existing array in the pool to fit the contents, create a new array
        return new T[capacity];
    }
    /// <summary>
    /// Releases an array back to the pool
    /// </summary>
    /// <param name="array">The array to release</param>
    public static void Release(ArraySegment<T> array) => Release(array.Array);
    /// <summary>
    /// Releases an array back to the pool
    /// </summary>
    /// <param name="array">The array to release</param>
    public static void Release(T[] array)
    {
        if (array == null)
            return;

        pool ??= new List<T[]>(10);
        pool.Add(array);
    }

    /// <summary>
    /// Disposes all the arrays currently not in use
    /// </summary>
    public static void DisposeAll()
    {
        pool?.Clear();
    }
}
