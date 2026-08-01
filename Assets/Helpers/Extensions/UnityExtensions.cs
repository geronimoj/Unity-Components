using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    public static void SafeSetActive(this GameObject obj, bool state)
    {
        if (obj)
            obj.SetActive(state);
    }
}
