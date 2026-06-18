using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.Utility
{
    /// <summary>
    /// Object for running Coroutines on
    /// </summary>
    public class EnumeratorObject : MonoBehaviour
    {
        static EnumeratorObject instance = null;
        static EnumeratorObject ddolInstance = null;

        public static EnumeratorObject Instance
        {
            get
            {
                if (!instance)
                {
                    instance = new GameObject("EnumeratorObject", typeof(EnumeratorObject)).GetComponent<EnumeratorObject>();
                }

                return instance;
            }
        }

        public static EnumeratorObject DDOLInstance
        {
            get
            {
                if (!ddolInstance)
                {
                    ddolInstance = new GameObject("EnumeratorObject", typeof(EnumeratorObject)).GetComponent<EnumeratorObject>();
                    GameObject.DontDestroyOnLoad(ddolInstance);
                }

                return ddolInstance;
            }
        }
    }
}