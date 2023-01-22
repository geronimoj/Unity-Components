using System;
using System.Collections.Generic;

public static class Extensions
{
    private static Random r = null;
    /// <summary>
    /// Is the string is null or empty
    /// </summary>
    /// <param name="string"></param>
    /// <returns>True if null or empty</returns>
    /// <remarks>This just calls string.IsNullOrEmpty() Primarily for convenience</remarks>
    public static bool IsNullOrEmpty(this string @string) => string.IsNullOrEmpty(@string);
    /// <summary>
    /// Is the string null or white spaces
    /// </summary>
    /// <param name="string"></param>
    /// <returns>True if string is null or white spaces</returns>
    /// <remarks>This just calls string.IsNullOrWhiteSpace() Primarily for convenience</remarks>
    public static bool IsNullOrWhiteSpace(this string @string) => string.IsNullOrWhiteSpace(@string);
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
    /// <summary>
    /// Checks if all values in a boolean array are true
    /// </summary>
    /// <param name="a"></param>
    /// <returns>Returns true if all values are true</returns>
    public static bool AllTrue(this bool[] a)
    {
        foreach (var b in a)
            if (!b)
                return false;

        return true;
    }
    /// <summary>
    /// Checks if all values in a boolean array are false
    /// </summary>
    /// <param name="a"></param>
    /// <returns>Returns true if all values are true</returns>
    public static bool AllFalse(this bool[] a)
    {
        foreach (var b in a)
            if (b)
                return false;

        return true;
    }
    /// <summary>
    /// Returns the number of true booleans in a boolean array
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int TrueCount(this bool[] a)
    {
        int count = 0;
        foreach (var b in a)
            if (b)
                count++;

        return count;
    }
    /// <summary>
    /// Returns the number of false booleans in a boolean array
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int FalseCount(this bool[] a)
    {
        int count = 0;
        foreach (var b in a)
            if (!b)
                count++;

        return count;
    }
    /// <summary>
    /// Returns a value between 0 - 1 representing the percentage of True values in the boolean array
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static float TruePercent(this bool[] a)
    {
        return TrueCount(a) / (float)a.Length;
    }
    /// <summary>
    /// Returns a value between 0 - 1 representing the percentage of False values in the boolean array
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static float FalsePercent(this bool[] a)
    {
        return FalseCount(a) / (float)a.Length;
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
    /// <summary>
    /// Round the floating point value up to the nearest integer value
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static int RoundUp(this float f)
    {   //If value is basically an integer type, use value but as int
        if ((f % 1).Equals(0f, 0.00001f))
            return (int)f;

        return (int)f + 1;
    }
    /// <summary>
    /// Round the floating point value down to the nearest integer value
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static int RoundDown(this float f)
    {
        return (int)f;
    }
    /// <summary>
    /// Round the double value up to the nearest integer value
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static int RoundUp(this double d)
    {
        if ((d % 1).Equals(0f, 0.00001f))
            return (int)d;

        return (int)d + 1;
    }
    /// <summary>
    /// Round the double value down to the nearest integer value
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static int RoundDown(this double d)
    {
        return (int)d;
    }
}