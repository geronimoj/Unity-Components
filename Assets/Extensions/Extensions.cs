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

public static class UnityExtensions
{
    public static bool DoesOverlap(this UnityEngine.Vector2 thisOrigin, UnityEngine.Vector2 thisDirection, UnityEngine.Vector2 otherOrigin, UnityEngine.Vector2 otherDirection, out UnityEngine.Vector2 overlapPoint)
    {
        thisDirection.Normalize();
        otherDirection.Normalize();
        float a1, a2;
        a1 = UnityEngine.Mathf.Atan2(thisDirection.y, thisDirection.x);
        a2 = UnityEngine.Mathf.Atan2(otherDirection.y, otherDirection.x);

        if (a1 >= Math.PI)
            a1 -= (float)Math.PI;
        if (a2 >= Math.PI)
            a2 -= (float)Math.PI;
        //Make sure lines are not parallel
        if (a1.Equals(a2, 0.001f))
        {
            overlapPoint = UnityEngine.Vector2.zero;
            return false;
        }

        a1 -= a2;
        a1 = Math.Abs(a1);

        overlapPoint = UnityEngine.Vector2.zero;
        //Check lines are not perpendicular
        if (a1.Equals((float)(Math.PI / 2), 0.001f))
        {   //Are perpendicular
            UnityEngine.Vector2 aToB = otherOrigin - thisOrigin;
            //Use Dot to get distance
            a1 = UnityEngine.Vector2.Dot(aToB, thisDirection);
            overlapPoint = thisOrigin + thisDirection * a1;
            return true;
        }

        //Check lines are not 1,0 or 0,1

        float c1, c2;

        throw new NotImplementedException("DoesOverlap not complete");
    }
}