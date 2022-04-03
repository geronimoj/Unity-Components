using System;
using System.Collections.Generic;

public static class Extensions
{
    private static Random r = null;
    /// <summary>
    /// Returns true if the array is null or empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this T[] array)
    {
        return array == null || array.Length == 0;
    }
    /// <summary>
    /// Returns true if the list is null or empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return list == null || list.Count == 0;
    }
    /// <summary>
    /// Returns true if the dictionary is null or empty
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> dic)
    {
        return dic == null || dic.Count == 0;
    }
    /// <summary>
    /// Gets a random item from the array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <returns>Returns a random item from the array. Returns default if array is empty or null</returns>
    public static T GetRandom<T>(this T[] a)
    {
        if (a.IsNullOrEmpty())
            return default;

        if (r == null)
            r = new Random();
        //r ??= new Random(); << Need to update c# to 8.0

        return a[r.Next(a.Length)];
    }
    /// <summary>
    /// Gets a random item from the list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="l"></param>
    /// <param name="remove">To remove the random item from the list or not</param>
    /// <returns>Returns a random item from the list. Returns default if list is empty or null</returns>
    public static T GetRandom<T>(this List<T> l, bool remove = false)
    {
        if (l.IsNullOrEmpty())
            return default;

        if (r == null)
            r = new Random();
        //r ??= new Random(); << Need to update c# to 8.0

        if (!remove)
            return l[r.Next(l.Count)];

        int index = r.Next(l.Count);

        T ret = l[index];
        l.RemoveAt(index);
        return ret;
    }

    #region Arrays
    /// <summary>
    /// Extends the array by extendAmount
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="extendAmount">The amount to extend by. Must be greater than 0</param>
    /// <returns>Returns a new array with the new length & indentical contents</returns>
    public static T[] Extend<T>(this T[] a, int extendAmount)
    {   //Must be extending by valid number
        if (extendAmount <= 0)
            throw new ArgumentException("extendAmount must be greater than 0");
        //If null or empty, return new array
        if (a.IsNullOrEmpty())
            return new T[extendAmount];

        extendAmount += a.Length;
        //The @ just lets me use new as a variable instead of a keyword
        T[] @new = new T[extendAmount];
        //Copy data over
        for (uint i = 0; i < extendAmount; i++)
            @new[i] = a[i];

        return @new;
    }
    /// <summary>
    /// Reduces the size of the array by shrinkAmount. Will discard data at the end of the array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="shrinkAmount">The amount to reduce by. Must be positive value</param>
    /// <returns>Returns a new array with the reduced size and any, not lost data</returns>
    public static T[] Shrink<T>(this T[] a, int shrinkAmount)
    {
        if (shrinkAmount <= 0)
            throw new ArgumentException("extendAmount must be greater than 0");

        if (a.IsNullOrEmpty())
            return a;
        //Get smallest value, clamped at 0
        shrinkAmount = Math.Max(0, a.Length - shrinkAmount);

        T[] @new = new T[shrinkAmount];
        //If size is not 0, copy data
        if (shrinkAmount != 0)
            for (int i = 0; i < shrinkAmount; i++)
                @new[i] = a[i];

        return @new;
    }
    #endregion

    #region IsEven
    public static bool IsEven(this byte b)
    {
        return b % 2 == 0;
    }
    public static bool IsEven(this sbyte b)
    {
        return b % 2 == 0;
    }
    public static bool IsEven(this short s)
    {
        return s % 2 == 0;
    }
    public static bool IsEven(this ushort s)
    {
        return s % 2 == 0;
    }
    public static bool IsEven(this int i)
    {
        return i % 2 == 0;
    }
    public static bool IsEven(this uint i)
    {
        return i % 2 == 0;
    }
    public static bool IsEven(this long l)
    {
        return l % 2 == 0;
    }
    public static bool IsEven(this ulong l)
    {
        return l % 2 == 0;
    }
    #endregion

    #region IsOdd
    public static bool IsOdd(this byte i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this sbyte i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this short i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this ushort i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this int i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this uint i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this long i)
    {
        return i % 2 == 1;
    }
    public static bool IsOdd(this ulong i)
    {
        return i % 2 == 1;
    }
    #endregion

    public static bool Equals(this float f, float target, float tolerance)
    {
        target -= f;
        target = Math.Abs(target);
        return target < tolerance;
    }

    public static bool Equals(this double d, double target, double tolerance)
    {
        target -= d;
        target = Math.Abs(target);
        return target < tolerance;
    }
}