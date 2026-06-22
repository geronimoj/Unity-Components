using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempArray<T> : IEnumerable<T>, IEnumerable
{
    /// <summary>
    /// static list of all active temp list references to avoid the PoolLists going to GC & getting resurrected.
    /// </summary>
    static List<object> softReferences = null;

    readonly ArraySegment<T> segment;

    public TempArray(int length)
    {
        segment = PoolArray<T>.Get(length);
        softReferences ??= new List<object>(10);
        softReferences.Add(segment.Array);
    }

    ~TempArray()
    {
        PoolArray<T>.Release(segment);
        softReferences.Remove(segment.Array);
    }

    public T this[int index]
    {
        get => segment[index];
        set => segment[index] = value;
    }

    public int Length => segment.Count;

    public T[] ToArray()
    {
        int length = segment.Count;
        T[] ret = new T[length];

        for (int i = 0; i < length; i++)
            ret[i] = (T)segment[i];

        return ret;
    }

    public List<T> ToList()
    {
        int length = segment.Count;
        List<T> ret = new List<T>(length);

        for (int i = 0; i < length; i++)
            ret.Add((T)segment[i]);

        return ret;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return segment.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
