using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoConditions
{
    public abstract class Condition<T> : ScriptableObject
    {
        /// <summary>
        /// Checked as often as specified
        /// </summary>
        /// <param name="obj">Data from connected monoconditions</param>
        /// <returns></returns>
        public abstract bool IsTrue(T obj);
    }
}