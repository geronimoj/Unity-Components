using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Animation event that executes so long as the clip is playing
    /// </summary>
    public abstract class AnimationEventPersistentParameter : AnimationEventParameter
    {
        /// <summary>
        /// Obtains a unique key for storing PersistentParameterData in the AnimationEventHandler
        /// </summary>
        /// <param name="handler">The animaton event handler processing this parameter</param>
        /// <param name="animationEvent">The animation event that contains this object</param>
        /// <returns></returns>
        public virtual object GetUniqueKey(AnimationEventHandler handler, AnimationEvent animationEvent)
        {
            // If blended, just use this object for key. This ensures it will be the same for blended events.
            if (isBlended)
                return this;
            else
                return animationEvent;
        }
        /// <summary>
        /// Execute the event
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="persistentData"></param>
        public abstract PersistentParameterData Update(AnimationEventHandler handler, PersistentParameterData persistentData, float weight);
    }

    /// <summary>
    /// Animation event that executes so long as the clip  it's attached too is playing.
    /// </summary>
    public abstract class AnimationEventPersistentParameter<TPersistentDataType> : AnimationEventPersistentParameter where TPersistentDataType : PersistentParameterData
    {
        /// <summary>
        /// Execute the event
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="persistentData"></param>
        public override PersistentParameterData Update(AnimationEventHandler handler, PersistentParameterData persistentData, float weight)
        {
            // Execute the Update type cast, to make derived classes easier to work with. Keep the last exections weight up to date.
            var ret = Update(handler, persistentData as TPersistentDataType, weight);
            ret.lastWeight = weight;

            return ret;
        }
        /// <summary>
        /// Execute the event
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="persistentData"></param>
        protected abstract TPersistentDataType Update(AnimationEventHandler handler, TPersistentDataType persistentData, float weight);
    }

    /// <summary>
    /// Base class for persistent data for AnimationEventPersistentData
    /// </summary>
    public class PersistentParameterData
    {
        /// <summary>
        /// The parameter this persistent data is ascociated with.
        /// </summary>
        public AnimationEventPersistentParameter parameter;
        /// <summary>
        /// The weight of the event the last time it was processed
        /// </summary>
        public float lastWeight = 0f;
        /// <summary>
        /// The frame this persistent data was last updated.
        /// </summary>
        /// <remarks>
        /// Used exclusively for determining if the Event already executed in LateUpdate for a specific frame.
        /// </remarks>
        public int lastUpdate;
    }
}