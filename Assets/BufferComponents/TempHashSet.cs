using System;
using System.Collections;
using System.Collections.Generic;

public class TempHashSet<T> : IEnumerable<T>, IEnumerable
{
    /// <summary>
    /// static list of all active temp list references to avoid the PoolLists going to GC & getting resurrected.
    /// </summary>
    static List<object> softReferences = null;

    PoolHashSet<T> hashSet = null;

    public TempHashSet(int capacity)
    {
        hashSet = PoolHashSet<T>.Get(capacity);

        softReferences ??= new List<object>(10);
        softReferences.Add(hashSet);
    }

    ~TempHashSet()
    {
        hashSet.Release();
        softReferences.Remove(hashSet);
    }

    public bool Add(T item) => hashSet.Add(item);
    public void Clear() => hashSet.Clear();
    public bool Contains(T item) => hashSet.Contains(item);
    public void CopyTo(T[] array) => hashSet.CopyTo(array);
    public void CopyTo(T[] array, int arrayIndex) => hashSet.CopyTo(array, arrayIndex);
    public void CopyTo(T[] array, int arrayIndex, int count) => hashSet.CopyTo(array, arrayIndex, count);
    public void ExceptWith(IEnumerable<T> other) => hashSet.ExceptWith(other);
    public void IntersectWith(IEnumerable<T> other) => hashSet.IntersectWith(other);
    public bool IsProperSubsetOf(IEnumerable<T> other) => hashSet.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => hashSet.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => hashSet.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => hashSet.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => hashSet.Overlaps(other);
    public bool Remove(T item) => hashSet.Remove(item);
    public int RemoveWhere(Predicate<T> match) => hashSet.RemoveWhere(match);
    public bool SetEquals(IEnumerable<T> other) => hashSet.SetEquals(other);
    public void SymmetricExceptWith(IEnumerable<T> other) => hashSet.SymmetricExceptWith(other);
    public bool TryGetValue(T equalValue, out T actualValue) => hashSet.TryGetValue(equalValue, out actualValue);
    public void UnionWith(IEnumerable<T> other) => hashSet.UnionWith(other);

    public IEnumerator<T> GetEnumerator()
    {
        return hashSet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
