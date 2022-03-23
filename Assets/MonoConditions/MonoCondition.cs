using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoConditions
{
    public abstract class MonoCondition<T> : MonoBehaviour
    {
        [SerializeField]
        private Condition<T>[] _conditions = null;

        [SerializeField]
        private Action<T>[] _actions = null;
#if RATE_LIMIT
        [SerializeField]
        [Tooltip("How many frames between each call")]
        private byte _callRate = 0;
#endif
        private T data = default;

        private void Awake()
        {
            if (_actions.IsNullOrEmpty() || _conditions.IsNullOrEmpty())
            {
                enabled = false;
                return;
            }

            data = Get();
#if RATE_LIMIT
            _callRate++;
#endif
        }
        /// <summary>
        /// Get data specific to this mono. Called in Awake
        /// </summary>
        /// <returns>Gets data for this mono</returns>
        protected abstract T Get();

        private void Update()
        {
#if RATE_LIMIT
            //Check time
            if (Time.frameCount % _callRate != 0)
                return;
#endif
            //Check conditions
            foreach (Condition<T> condition in _conditions)
                if (!condition.IsTrue(data))
                    return;
            //Active actions
            foreach (Action<T> action in _actions)
                action.Execute(data, gameObject);
            //Disable update
            enabled = false;
        }
    }
}