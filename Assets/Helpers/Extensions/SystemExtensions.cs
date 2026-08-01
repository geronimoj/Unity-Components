// Created by Luke Jones 01/06/22

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemExtensions
{
    #region ActionExtensions
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
    #endregion

    #region BooleanCompression
    /// <summary>
    /// Stores an array of bools in a byte or byte array. Allocates a new array
    /// </summary>
    /// <param name="bools"></param>
    /// <returns></returns>
    public static byte[] StoreBools(params bool[] bools)
    {
        int length = bools.Length / 8;
        //Clamp length
        if (length <= 0)
            length = 1;

        byte[] bytes = new byte[length];
        //Use nonalloc version for math.
        StoreBoolsNonAlloc(bytes, bools);

        return bytes;
    }
    /// <summary>
    /// Stores an array of bools in a byte array. Will not allocate memory.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="bools"></param>
    /// <exception cref="IndexOutOfRangeException">Throws an exception if there isn't enough space in bytes for all the bools.</exception>
    public static void StoreBoolsNonAlloc(in byte[] bytes, params bool[] bools)
    {
        int prevI = -1;
        for (int b = 0; b < bools.Length; b++)
        {
            int i = b / 8;
            //Not enough space left
            if (i >= bytes.Length)
                throw new IndexOutOfRangeException("Not enough bytes to store bools");
            //New index so quickly flush it
            if (i != prevI)
            {
                prevI = i;
                bytes[i] = 0b_0000_0000; // Fancy way of writing 0
            }
            //Should give us an index starting from the first byte in bytes[i]
            int temp = b - (i * 8);

            int valueAsByte = bools[b] ? 0b_0000_0001 : 0b_0000_0000; // Can't be bothered manually casting.
            //Shift across to correct bit
            byte t2 = (byte)(valueAsByte << temp);
            //Store
            bytes[i] |= t2;
        }
    }
    /// <summary>
    /// Reads a byte[] as if it were compressed bools. Allocates a new bool array
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="bools">The individual bits of the bools</param>
    public static void GetBools(this byte[] bytes, out bool[] bools)
    {   //Fill data
        bools = new bool[bytes.Length * 8];
        //Let the non alloc version do everything else
        bytes.GetBoolsNonAlloc(bools);
    }
    /// <summary>
    /// Reads a byte[] as if it were compressed bools. Allocates a new bool array
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="bools">The individual bits of the bools</param>
    /// <exception cref="IndexOutOfRangeException">Throws an exception if there isn't enough space in bools for all the bools.</exception>
    public static void GetBoolsNonAlloc(this byte[] bytes, in bool[] bools)
    {   //Fill data
        int trueIndex = 0;
        foreach (byte b in bytes)
            for (int i = 0; i < 8; i++)
            {
                if (trueIndex >= bools.Length)
                    throw new IndexOutOfRangeException("Array too short. Cannot store all bools");
                //Copy it to a safe place so we can mess around with it
                byte temp = b;
                //Bit shift so the value we are looking for is in the 1's column
                int t = temp >> i;
                //Simplest way to check if the 1 bit is 1 or 0.
                //Idk if this is any faster than manually bit shifting to flush
                bools[trueIndex] = t % 2 == 1;
                trueIndex++;
            }
    }

    //Note: I should probably make long, int, short and byte variants of these functions to allow for non-byte array storage.
    /// <summary>
    /// Stores up to 8 bools in a single byte. (Pretty sure it doesn't allocate memory)
    /// </summary>
    /// <param name="bools"></param>
    /// <returns></returns>
    public static byte StoreBoolsInByte(params bool[] bools)
    {
        byte ret = 0;
        for (int b = 0; b < bools.Length; b++)
        {   //Can only store 8 bools
            if (b >= 8)
                return ret;
            //Not enough space left
            //Should give us an index starting from the first byte in bytes[i]
            int temp = b;

            int valueAsByte = bools[b] ? 0b_0000_0001 : 0b_0000_0000; // Can't be bothered manually casting.
            //Shift across to correct bit
            byte t2 = (byte)(valueAsByte << temp);
            //Store
            ret |= t2;
        }
        return ret;
    }
    /// <summary>
    /// Reads the bits of a byte as a bool[] for compressed data storage. DOES NOT ALLOCATE MEMORY IF inBools ASSIGNED
    /// </summary>
    /// <param name="b"></param>
    /// <param name="inBools">Assign if you want to avoid memory allocation. Will write to this array (output in outBools)</param>
    /// <param name="outBools">Set to inBools if inBools is assigned, then filled</param>
    public static void GetBools(this byte b, in bool[] inBools, out bool[] outBools)
    {   //Setup out bool
        if (inBools == null)
            outBools = new bool[8];
        else
            outBools = inBools;

        for (int i = 0; i < 8; i++)
        {   //Out of space to assign to exit now.
            if (i >= outBools.Length)
                return;
            //Copy it to a safe place so we can mess around with it
            byte temp = b;
            //Bit shift so the value we are looking for is in the 1's column
            int t = temp >> i;
            //Simplest way to check if the 1 bit is 1 or 0.
            //Idk if this is any faster than manually bit shifting to flush
            outBools[i] = t % 2 == 1;
        }
    }
    /// <summary>
    /// Stores up to 16 bools in a single short. (Pretty sure it doesn't allocate memory)
    /// </summary>
    /// <param name="bools"></param>
    /// <returns></returns>
    public static short StoreBoolsInShort(params bool[] bools)
    {
        short ret = 0;
        for (int b = 0; b < bools.Length; b++)
        {   //Can only store 8 bools
            if (b >= 16)
                return ret;
            //Not enough space left
            //Should give us an index starting from the first byte in bytes[i]
            int temp = b;

            int valueAsByte = bools[b] ? 0b_0000_0001 : 0b_0000_0000; // Can't be bothered manually casting.
            //Shift across to correct bit
            short t2 = (short)(valueAsByte << temp);
            //Store
            ret |= t2;
        }
        return ret;
    }
    /// <summary>
    /// Reads the bits of a byte as a bool[] for compressed data storage. DOES NOT ALLOCATE MEMORY IF inBools ASSIGNED
    /// </summary>
    /// <param name="b"></param>
    /// <param name="inBools">Assign if you want to avoid memory allocation. Will write to this array (output in outBools)</param>
    /// <param name="outBools">Set to inBools if inBools is assigned, then filled</param>
    public static void GetBools(this short b, in bool[] inBools, out bool[] outBools)
    {   //Setup out bool
        if (inBools == null)
            outBools = new bool[16];
        else
            outBools = inBools;

        for (int i = 0; i < 16; i++)
        {   //Out of space to assign to exit now.
            if (i >= outBools.Length)
                return;
            //Copy it to a safe place so we can mess around with it
            short temp = b;
            //Bit shift so the value we are looking for is in the 1's column
            int t = temp >> i;
            //Simplest way to check if the 1 bit is 1 or 0.
            //Idk if this is any faster than manually bit shifting to flush
            outBools[i] = t % 2 == 1;
        }
    }
    /// <summary>
    /// Stores up to 16 bools in a single short. (Pretty sure it doesn't allocate memory)
    /// </summary>
    /// <param name="bools"></param>
    /// <returns></returns>
    public static int StoreBoolsInInt(params bool[] bools)
    {
        int ret = 0;
        for (int b = 0; b < bools.Length; b++)
        {   //Can only store 8 bools
            if (b >= 32)
                return ret;
            //Not enough space left
            //Should give us an index starting from the first byte in bytes[i]
            int temp = b;

            int valueAsByte = bools[b] ? 0b_0000_0001 : 0b_0000_0000; // Can't be bothered manually casting.
            //Shift across to correct bit
            int t2 = (int)(valueAsByte << temp);
            //Store
            ret |= t2;
        }
        return ret;
    }
    /// <summary>
    /// Reads the bits of a byte as a bool[] for compressed data storage. DOES NOT ALLOCATE MEMORY IF inBools ASSIGNED
    /// </summary>
    /// <param name="b"></param>
    /// <param name="inBools">Assign if you want to avoid memory allocation. Will write to this array (output in outBools)</param>
    /// <param name="outBools">Set to inBools if inBools is assigned, then filled</param>
    public static void GetBools(this int b, in bool[] inBools, out bool[] outBools)
    {   //Setup out bool
        if (inBools == null)
            outBools = new bool[32];
        else
            outBools = inBools;

        for (int i = 0; i < 32; i++)
        {   //Out of space to assign to exit now.
            if (i >= outBools.Length)
                return;
            //Copy it to a safe place so we can mess around with it
            int temp = b;
            //Bit shift so the value we are looking for is in the 1's column
            int t = temp >> i;
            //Simplest way to check if the 1 bit is 1 or 0.
            //Idk if this is any faster than manually bit shifting to flush
            outBools[i] = t % 2 == 1;
        }
    }
    /// <summary>
    /// Stores up to 16 bools in a single short. (Pretty sure it doesn't allocate memory)
    /// </summary>
    /// <param name="bools"></param>
    /// <returns></returns>
    public static long StoreBoolsInLong(params bool[] bools)
    {
        long ret = 0;
        for (int b = 0; b < bools.Length; b++)
        {   //Can only store 8 bools
            if (b >= 64)
                return ret;
            //Not enough space left
            //Should give us an index starting from the first byte in bytes[i]
            int temp = b;

            long valueAsByte = bools[b] ? 0b_0000_0001 : 0b_0000_0000; // Can't be bothered manually casting.
            //Shift across to correct bit
            long t2 = (long)(valueAsByte << temp);
            //Store
            ret |= t2;
        }
        return ret;
    }
    /// <summary>
    /// Reads the bits of a byte as a bool[] for compressed data storage. DOES NOT ALLOCATE MEMORY IF inBools ASSIGNED
    /// </summary>
    /// <param name="b"></param>
    /// <param name="inBools">Assign if you want to avoid memory allocation. Will write to this array (output in outBools)</param>
    /// <param name="outBools">Set to inBools if inBools is assigned, then filled</param>
    public static void GetBools(this long b, in bool[] inBools, out bool[] outBools)
    {   //Setup out bool
        if (inBools == null)
            outBools = new bool[64];
        else
            outBools = inBools;

        for (int i = 0; i < 64; i++)
        {   //Out of space to assign to exit now.
            if (i >= outBools.Length)
                return;
            //Copy it to a safe place so we can mess around with it
            long temp = b;
            //Bit shift so the value we are looking for is in the 1's column
            long t = temp >> i;
            //Simplest way to check if the 1 bit is 1 or 0.
            //Idk if this is any faster than manually bit shifting to flush
            outBools[i] = t % 2 == 1;
        }
    }
    #endregion

    #region Boolean Extract/Intert
    /// <summary>
    /// Extract a bit from a byte array.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool Extract(byte[] bytes, int index)
    {
        if (index > bytes.Length * 8)
            throw new IndexOutOfRangeException();

        int i1 = index / 8;
        int i2 = index % 8;

        byte b = bytes[i1];
        b >>= i2;

        return b % 2 == 1;
    }
    /// <summary>
    /// Sets a specific bit in the byte array
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public static void Insert(in byte[] bytes, int index, bool value)
    {
        if (index > bytes.Length * 8)
            throw new IndexOutOfRangeException();
        //Get index
        int i1 = index / 8;
        int i2 = index % 8;

        byte valueAsByte = (byte)(value ? 0b_0000_0001 : 0b_0000_0000); // Can't be bothered manually casting.
        valueAsByte <<= i2;

        byte flush = 0b_1111_1111;
        flush ^= valueAsByte;
        //This should leave all bits unchanged except for the bit we are about to edit
        bytes[i1] &= flush;
        bytes[i1] |= valueAsByte;
    }
    /// <summary>
    /// Extract a bit from a byte.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool Extract(this byte bytes, int index)
    {
        if (index > 8)
            throw new IndexOutOfRangeException();

        byte b = bytes;
        b >>= index;

        return b % 2 == 1;
    }
    /// <summary>
    /// Sets a specific bit in the byte
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public static void Insert(this ref byte bytes, int index, bool value)
    {
        if (index > 8)
            throw new IndexOutOfRangeException();

        byte valueAsByte = (byte)(value ? 0b_0000_0001 : 0b_0000_0000); // Can't be bothered manually casting.
        valueAsByte <<= index;

        byte flush = 0b_1111_1111;
        flush ^= valueAsByte;
        //This should leave all bits unchanged except for the bit we are about to edit
        bytes &= flush;
        bytes |= valueAsByte;
    }
    /// <summary>
    /// Extract a bit from a short.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool Extract(this short bytes, int index)
    {
        if (index > 16)
            throw new IndexOutOfRangeException();

        short b = bytes;
        b >>= index;

        return b % 2 == 1;
    }
    /// <summary>
    /// Sets a specific bit in the short
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public static void Insert(this ref short bytes, int index, bool value)
    {
        if (index > 16)
            throw new IndexOutOfRangeException();

        short valueAsByte = (byte)(value ? 0b_0000_0001 : 0b_0000_0000); // Can't be bothered manually casting.
        valueAsByte <<= index;

        short flush = 0b_1111_1111;
        flush ^= valueAsByte;
        //This should leave all bits unchanged except for the bit we are about to edit
        bytes &= flush;
        bytes |= valueAsByte;
    }
    /// <summary>
    /// Extract a bit from an int.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool Extract(this int bytes, int index)
    {
        if (index > 32)
            throw new IndexOutOfRangeException();

        int b = bytes;
        b >>= index;

        return b % 2 == 1;
    }
    /// <summary>
    /// Sets a specific bit in the int
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public static void Insert(this ref int bytes, int index, bool value)
    {
        if (index > 32)
            throw new IndexOutOfRangeException();

        int valueAsByte = (value ? 0b_0000_0001 : 0b_0000_0000); // Can't be bothered manually casting.
        valueAsByte <<= index;

        int flush = 0b_1111_1111;
        flush ^= valueAsByte;
        //This should leave all bits unchanged except for the bit we are about to edit
        bytes &= flush;
        bytes |= valueAsByte;
    }
    /// <summary>
    /// Extract a bit from a long.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static bool Extract(this long bytes, int index)
    {
        if (index > 64)
            throw new IndexOutOfRangeException();

        long b = bytes;
        b >>= index;

        return b % 2 == 1;
    }
    /// <summary>
    /// Sets a specific bit in the long
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public static void Insert(this ref long bytes, int index, bool value)
    {
        if (index > 32)
            throw new IndexOutOfRangeException();

        long valueAsByte = (value ? 0b_0000_0001 : 0b_0000_0000); // Can't be bothered manually casting.
        valueAsByte <<= index;

        long flush = 0b_1111_1111;
        flush ^= valueAsByte;
        //This should leave all bits unchanged except for the bit we are about to edit
        bytes &= flush;
        bytes |= valueAsByte;
    }
    #endregion
}