using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoConditions
{
    public abstract class Action<T> : ScriptableObject
    {
        /// <summary>
        /// Called by MonoCondition when all conditions have passed.
        /// </summary>
        /// <param name="obj">The data on the mono</param>
        /// <param name="object">The gameObject the mono was on</param>
        public abstract void Execute(T obj, GameObject @object);
    }
}