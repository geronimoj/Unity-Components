using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Parameter that executes child parameters when invoked
    /// </summary>
    [CreateAssetMenu(fileName = "Event_NAME_Multi_Parameters", menuName = "VFlame/Animation/Events/Multi")]
    public class AnimationEventMultiParameter : AnimationEventParameter
    {
        /// <summary>
        /// The parameters to execute
        /// </summary>
        [SerializeField] AnimationEventParameter[] parameters;

        /// <summary>
        /// Invoke each event in the animation paramters
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="weight"></param>
        public virtual void InvokeParameters(AnimationEventHandler handler, float weight)
        {
            // Loop over each parameter in this event and process it.
            foreach(var param in parameters)
            {
                // If the param is null, log an error. This is probably caused by a deleted asset that was still in use.
                if (param == null)
                {
                    Debug.LogError("[VFlame.AnimationEvent] Null Parameter for event: " + name, handler);
                    continue;
                }

                // Invoke each parameter
                handler.PlayEvent(param, weight);
            }
        }
    }
}