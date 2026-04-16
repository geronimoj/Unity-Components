using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Abstract class for implementing your own custom implementation for parameters
    /// </summary>
    public abstract class AnimationEventCustomParameter : AnimationEventParameter
    {
        /// <summary>
        /// Execute the animation event
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="weight"></param>
        public abstract void Invoke(AnimationEventHandler handler, float weight);
    }
}