// Created by Luke Jones 01/06/22

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemExtensions
{
    /// <summary>
    /// Null catches & try catches action
    /// </summary>
    /// <param name="action"></param>
    public static void SafeInvoke(this Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// Null catches & try catches action
    /// </summary>
    /// <param name="action"></param>
    public static void SafeInvoke<T>(this Action<T> action, T t)
    {
        try
        {
            action?.Invoke(t);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// Null catches & try catches action
    /// </summary>
    /// <param name="action"></param>
    public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 t1, T2 t2)
    {
        try
        {
            action?.Invoke(t1, t2);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// Null catches & try catches action
    /// </summary>
    /// <param name="action"></param>
    public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
    {
        try
        {
            action?.Invoke(t1, t2, t3);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// Null catches & try catches action
    /// </summary>
    /// <param name="action"></param>
    public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
    {
        try
        {
            action?.Invoke(t1, t2, t3, t4);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    //Yea I can't be bothered to do this all the way up to 16. Theres probably a better way anyways
}