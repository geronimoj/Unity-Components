using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BufferDictionary
{
    internal static Dictionary<object, object> buffer = null;
    /// <summary>
    /// Creates a structure for accessing the buffer
    /// </summary>
    /// <typeparam name="TKey">Key type of the dictionary</typeparam>
    /// <typeparam name="TValue">Value type pf the dictionary</typeparam>
    /// <param name="minCapacity">The minimum capacity</param>
    /// <returns></returns>
    public static BufferDictionary<TKey, TValue> Get<TKey, TValue>(int minCapacity)
    {
        return new BufferDictionary<TKey, TValue>(minCapacity);
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

public struct BufferDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
    #region Static Functions
    /// <summary>
    /// Creates a structure for accessing the buffer
    /// </summary>
    /// <typeparam name="TKey">Key type of the dictionary</typeparam>
    /// <typeparam name="TValue">Value type pf the dictionary</typeparam>
    /// <param name="minCapacity">The minimum capacity</param>
    /// <returns></returns>
    public static BufferDictionary<TKey, TValue> Get(int minCapacity)
    {
        return BufferDictionary.Get<TKey, TValue>(minCapacity);
    }
    /// <summary>
    /// Deletes the buffer, releasing it to GC
    /// </summary>
    public static void Dispose() => BufferList.Dispose();
    #endregion

    public readonly TValue this[TKey key]
    {
        get
        {
            // Throw relevant exceptions when directly indexing
            if (BufferDictionary.buffer == null)
                throw new NullReferenceException("Buffer is null");

            return (TValue)BufferDictionary.buffer[key];
        }
        set
        {
            // Throw relevant exceptions when directly indexing
            if (BufferDictionary.buffer == null)
                throw new NullReferenceException("Buffer is null");

            BufferDictionary.buffer[key] = value;
        }
    }
    /// <summary>
    /// The Count of the buffer
    /// </summary>
    public readonly int Count => BufferDictionary.buffer?.Count ?? 0;
    /// <summary>
    /// The Keys in the dictionary
    /// </summary>
    public readonly KeyCollection Keys => new KeyCollection();
    /// <summary>
    /// The Values in the dictionary
    /// </summary>
    public readonly ValueCollection Values => new ValueCollection();

    public readonly bool IsReadOnly => false;

    public BufferDictionary(int minCapacity)
    {
        // Allocate the buffer if necessary & make sure it can fit the dictionary
        BufferDictionary.buffer ??= new Dictionary<object, object>(minCapacity);
        BufferDictionary.buffer.EnsureCapacity(minCapacity);
    }
    /// <summary>
    /// Add an item to the dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public readonly void Add(TKey key, TValue value) => BufferDictionary.buffer.Add(key, value);
    public readonly void Add(KeyValuePair<TKey, TValue> item) => BufferDictionary.buffer.Add(item.Key, item.Value);
    /// <summary>
    /// Try add an item to the dictionary. Fails if there is already that value in the dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public readonly bool TryAdd(TKey key, TValue value) => BufferDictionary.buffer.TryAdd(key, value);
    public readonly bool TryGetValue(TKey key, out TValue value)
    {
        bool ret = BufferDictionary.buffer.TryGetValue(key, out object outVal);

        if (ret)
        {
            value = (TValue)outVal;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
    /// <summary>
    /// Clears the buffer
    /// </summary>
    public readonly void Clear() => BufferDictionary.Clear();
    public readonly bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out TValue value) && value.Equals(item.Value);
    }
    public readonly bool ContainsKey(TKey key) => BufferDictionary.buffer.ContainsKey(key);
    public readonly bool ContainsValue(TValue value) => BufferDictionary.buffer.ContainsValue(value);

    public readonly bool Remove(TKey key) => BufferDictionary.buffer.Remove(key);
    public readonly bool Remove(TKey key, out TValue value)
    {
        bool ret = BufferDictionary.buffer.Remove(key, out object outVal);

        if (ret)
        {
            value = (TValue)outVal;
            return ret;
        }
        else
        {
            value = default;
            return false;
        }
    }
    public readonly bool Remove(KeyValuePair<TKey, TValue> item)
    {
        // I think we need to sanity check value. If the Contains() passes, then the Remove should also perform & pass
        return Contains(item) && Remove(item.Key);
    }
    public readonly void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
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

    public readonly Dictionary<TKey, TValue> ToDictionary()
    {
        int count = BufferDictionary.buffer.Count;
        Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(count);

        // Write the data in using our already type cases enumerator
        foreach(var kvp in this)
            ret[kvp.Key] = kvp.Value;

        return ret;
    }

    public readonly IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new Enumerator(BufferDictionary.buffer);
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
    {
        Dictionary<object, object>.Enumerator enumerator;

        public Enumerator(Dictionary<object, object> buffer)
        {   // Just get an enumerator for the dictionary & forward everything through that.
            // We will simply cast the KVP to the required types on request
            enumerator = buffer.GetEnumerator();
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {   // Take the current KVP of the normal dictionary enumerator & cast them to the required type
                var current = enumerator.Current;
                return new KeyValuePair<TKey, TValue>((TKey)current.Key, (TValue)current.Value);
            }
        }

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

    /// <summary>
    /// Key collection enumeratable for BufferDictionary
    /// </summary>
    public struct KeyCollection : IEnumerable<TKey>, IEnumerable
    {
        /// <summary>
        /// The Count of the buffer
        /// </summary>
        public readonly int Count => BufferDictionary.buffer?.Count ?? 0;

        public readonly IEnumerator<TKey> GetEnumerator()
        {
            // Create the enumerator, using the Buffer Dictionaries enumerator as a base.
            return new Enumerator(new BufferDictionary<TKey, TValue>.Enumerator(BufferDictionary.buffer));
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerator for KeyCollection
        /// </summary>
        public struct Enumerator : IEnumerator<TKey>, IEnumerator
        {
            BufferDictionary<TKey, TValue>.Enumerator enumerator;

            public Enumerator(BufferDictionary<TKey, TValue>.Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public TKey Current => enumerator.Current.Key;

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
                enumerator.Reset();
            }
        }
    }
    /// <summary>
    /// Key collection enumeratable for BufferDictionary
    /// </summary>
    public struct ValueCollection : IEnumerable<TValue>, IEnumerable
    {
        /// <summary>
        /// The Count of the buffer
        /// </summary>
        public readonly int Count => BufferDictionary.buffer?.Count ?? 0;

        public readonly IEnumerator<TValue> GetEnumerator()
        {
            // Create the enumerator, using the Buffer Dictionaries enumerator as a base.
            return new Enumerator(new BufferDictionary<TKey, TValue>.Enumerator(BufferDictionary.buffer));
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerator for ValueCollection
        /// </summary>
        public struct Enumerator : IEnumerator<TValue>, IEnumerator
        {
            BufferDictionary<TKey, TValue>.Enumerator enumerator;

            public Enumerator(BufferDictionary<TKey, TValue>.Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public TValue Current => enumerator.Current.Value;

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
                enumerator.Reset();
            }
        }
    }
    /* Functions to implement with Typed handling
    ICollection<KeyValuePair<TKey, TValue>> , IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, ICollection, IDictionary
    
    public KeyCollection Keys { get; }
    public ValueCollection Values { get; }
    public IEqualityComparer<TKey> Comparer { get; }
    */
}
