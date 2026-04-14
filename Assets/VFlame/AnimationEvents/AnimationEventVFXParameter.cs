using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Base Parameter object for VFX from AnimationEvents
    /// </summary>
    public abstract class AnimationEventVFXParameter : AnimationEventParameter
    {
        /// <summary>
        /// Get the parent object to spawn the prefab at
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public abstract Transform GetParent(AnimationEventHandler handler);
        /// <summary>
        /// Should the prefab attach to the parent or just copy its position
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public abstract bool GetAttachToParent(AnimationEventHandler handler, float weight);
        /// <summary>
        /// Get the lifespan of the spawned object. 0 for endless life!
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public abstract float GetLifespan(AnimationEventHandler handler, float weight);
        /// <summary>
        /// Get the prefab for this event
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public abstract GameObject GetPrefab(AnimationEventHandler handler, float weight);
    }
}