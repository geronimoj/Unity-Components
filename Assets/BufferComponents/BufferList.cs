using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BufferList<T>
{
    private static List<T> buffer = null;

    public static List<T> Get(int maxCapacity)
    {
        // Allocate the buffer if necessary
        if (buffer == null)
            buffer = new List<T>(maxCapacity);
        else if (buffer.Capacity < maxCapacity)
        {
            buffer.Capacity = maxCapacity;
        }

        // Just clear it just in case they need it that way
        buffer.Clear();
        return buffer;
    }
    /// <summary>
    /// Empties the buffer
    /// </summary>
    public static void Clear()
    {
        buffer?.Clear();
    }
    /// <summary>
    /// Deletes the buffer, releasing it to GC
    /// </summary>
    public static void Dispose()
    {
        buffer = null;
    }
}
