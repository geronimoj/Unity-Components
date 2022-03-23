using System.Collections.Generic;

public static class Extensions
{
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
}
