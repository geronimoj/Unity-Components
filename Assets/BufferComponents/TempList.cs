using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a wrapper for a pool list. It will automatically release the internal pool list back to the pool when GC finalizes it.
/// </summary>
/// <remarks>
/// This object is designed for temporary allocation of a pool list, mitigating the GC overheal caused by creating lists
/// </remarks>
/// <typeparam name="T"></typeparam>
public class TempList<T> : IEnumerable<T>, IEnumerable
{
    /// <summary>
    /// static list of all active temp list references to avoid the PoolLists going to GC & getting resurrected.
    /// </summary>
    static List<PoolList<T>> softReferences = null;

    /// <summary>
    /// Internal pool list to act as the actual list the logic is interacting with.
    /// </summary>
    readonly PoolList<T> list;

    public TempList(int capacity)
    {   // Grab a list from the pool to avoid having to alloc an entire list. TempList is a much smaller GC footprint.
        list = PoolList<T>.Get(capacity);

        // Add the list to the softReferences to protected it from resurrection during the TempList destructor.
        softReferences ??= new List<PoolList<T>>(10);
        softReferences.Add(list);
    }

    ~TempList()
    {
        // Release the list back to the pool. It can now be removed from softReferences as it's stored in a separate list now.
        list.Release();
        softReferences.Remove(list);
    }

    public T this[int index]
    {
        get => list[index];
        set => list[index] = value;
    }

    public int Count => list.Count;

    public int Capacity
    {
        get => list.Capacity;
        set => list.Capacity = value;
    }

    public void Add(T item) => list.Add(item);
    public void AddRange(IEnumerable<T> collection) => list.AddRange(collection);

    public bool Contains(T item) => list.Contains(item);

    public void CopyTo(T[] array) => list.CopyTo(array);
    public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
    public void CopyTo(int index, T[] array, int arrayIndex, int count) => list.CopyTo(index, array, arrayIndex, count);

    public void Clear() => list.Clear();

    public void Sort(Comparison<T> comparison) => list.Sort(comparison);
    public void Sort(IComparer<T> comparer) => list.Sort(comparer);
    public void Sort(int index, int count, IComparer<T> comparer) => list.Sort(index, count, comparer);

    public int BinarySearch(T item) => list.BinarySearch(item);
    public int BinarySearch(T item, IComparer<T> comparer) => list.BinarySearch(item, comparer);
    public int BinarySearch(int index, int count, T item, IComparer<T> comparer) => list.BinarySearch(index, count, item, comparer);

    public bool Exists(Predicate<T> match) => list.Exists(match);
    public T Find(Predicate<T> match) => list.Find(match);
    public List<T> FindAll(Predicate<T> match) => list.FindAll(match);
    public T FindLast(Predicate<T> match) => list.FindLast(match);
    public int FindIndex(Predicate<T> match) => list.FindIndex(match);
    public int FindIndex(int index, Predicate<T> match) => list.FindIndex(index, match);
    public int FindIndex(int index, int count, Predicate<T> match) => list.FindIndex(index, count, match);

    public int IndexOf(T item) => list.IndexOf(item);
    public int IndexOf(T item, int index) => list.IndexOf(item, index);
    public int IndexOf(T item, int index, int count) => list.IndexOf(item, index, count);
    public int LastIndexOf(T item) => list.LastIndexOf(item);
    public int LastIndexOf(T item, int index) => list.LastIndexOf(item, index);
    public int LastIndexOf(T item, int index, int count) => list.LastIndexOf(item, index, count);
    public int FindLastIndex(Predicate<T> match) => list.FindLastIndex(match);
    public int FindLastIndex(int startIndex, Predicate<T> match) => list.FindLastIndex(0, match);
    public int FindLastIndex(int startIndex, int count, Predicate<T> match) => list.FindLastIndex(startIndex, count, match);

    public void Insert(int index, T item) => list.Insert(index, item);
    public void InsertRange(int index, IEnumerable<T> collection) => list.InsertRange(index, collection);

    public void ForEach(Action<T> action) => list.ForEach(action);
    public bool TrueForAll(Predicate<T> match) => list.TrueForAll(match);

    public List<T> GetRange(int index, int count) => list.GetRange(index, count);

    public bool Remove(T item) => list.Remove(item);
    public int RemoveAll(Predicate<T> match) => list.RemoveAll(match);
    public void RemoveAt(int index) => list.RemoveAt(index);
    public void RemoveRange(int index, int count) => list.RemoveRange(index, count);
    public void Reverse() => list.Reverse();
    public void Reverse(int index, int count) => list.Reverse(index, count);

    public T[] ToArray() => list.ToArray();
    public List<T> ToList() => new List<T>(list);

    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => list.ConvertAll(converter);

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
}

