using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is a wrapper for a pool dictioanry. It will automatically release the internal pool dictioanry back to the pool when GC finalizes it.
/// </summary>
/// <remarks>
/// This object is designed for temporary allocation of a dictioanry list, mitigating the GC overheal caused by creating dictioanry
/// </remarks>
/// <typeparam name="T"></typeparam>
public class TempDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    /// static list of all active temp list references to avoid the PoolLists going to GC & getting resurrected.
    /// </summary>
    static List<object> softReferences = null;

    /// <summary>
    /// The internal dictionary
    /// </summary>
    PoolDictionary<TKey, TValue> dictionary = null;

    public TempDictionary(int capacity)
    {   // Grab a list from the pool to avoid having to alloc an entire list. TempList is a much smaller GC footprint.
        dictionary = PoolDictionary<TKey, TValue>.Get(capacity);

        // Add the list to the softReferences to protected it from resurrection during the TempList destructor.
        softReferences ??= new List<object>(10);
        softReferences.Add(dictionary);
    }

    ~TempDictionary()
    {
        // Release the dictionary back to the pool then remove it from softReferences so it doesn't get held in memory
        dictionary.Release();
        softReferences.Remove(dictionary);
    }

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }
    /// <summary>
    /// The Count of the buffer
    /// </summary>
    public int Count => dictionary.Count;
    /// <summary>
    /// The Keys in the dictionary
    /// </summary>
    public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;
    /// <summary>
    /// The Values in the dictionary
    /// </summary>
    public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;

    public bool IsReadOnly => false;

    /// <summary>
    /// Add an item to the dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(TKey key, TValue value) => dictionary.Add(key, value);
    public void Add(KeyValuePair<TKey, TValue> item) => dictionary.Add(item.Key, item.Value);
    /// <summary>
    /// Try add an item to the dictionary. Fails if there is already that value in the dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryAdd(TKey key, TValue value) => dictionary.TryAdd(key, value);
    public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);
    /// <summary>
    /// Clears the buffer
    /// </summary>
    public void Clear() => dictionary.Clear();
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out TValue value) && value.Equals(item.Value);
    }
    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
    public bool ContainsValue(TValue value) => dictionary.ContainsValue(value);

    public bool Remove(TKey key) => dictionary.Remove(key);
    public bool Remove(TKey key, out TValue value) => dictionary.Remove(key, out value);
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        // I think we need to sanity check value. If the Contains() passes, then the Remove should also perform & pass
        return Contains(item) && Remove(item.Key);
    }
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {   // Invalid starting index. Just throw an 
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException("Index is less than zero");

        foreach (var kvp in this)
        {   // Stop copy on out of range.
            if (arrayIndex >= array.Length)
                break;

            array[arrayIndex] = kvp;
            arrayIndex++;
        }
    }

    public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(dictionary);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
