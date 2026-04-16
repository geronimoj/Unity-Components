using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Parameter object for VFX from AnimationEvents
    /// </summary>
    [CreateAssetMenu(fileName = "Event_NAME_VFX_Parameters", menuName = "VFlame/Animation/Events/VFX")]
    public class AnimationEventVFXParameterScriptable : AnimationEventVFXParameter
    {
        /// <summary>
        /// The Id of the spawn point
        /// </summary>
        [SerializeField]
        protected string transformParentId;
        /// <summary>
        /// Should the event attach itself to the parent
        /// </summary>
        [SerializeField]
        protected bool attachToParent;
        /// <summary>
        /// The lifespan of the VFX event
        /// </summary>
        [SerializeField]
        protected float lifespan;
        /// <summary>
        /// The prefab for the VFX to spawn
        /// </summary>
        [SerializeField]
        protected GameObject prefab;

        /// <summary>
        /// Get the parent object to spawn the prefab at
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public override Transform GetParent(AnimationEventHandler handler)
        {
            if (handler.parents.TryGetValue(transformParentId, out var transform))
                return transform;

            // Default to the AnimationEventHandler so they don't just spawn at 0,0,0
            return handler.transform;
        }
        /// <summary>
        /// Should the prefab attach to the parent or just copy its position
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public override bool GetAttachToParent(AnimationEventHandler handler, float weight)
        {
            return attachToParent;
        }
        /// <summary>
        /// Get the lifespan of the spawned object. 0 for endless life!
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public override float GetLifespan(AnimationEventHandler handler, float weight)
        {
            return lifespan;
        }
        /// <summary>
        /// Get the prefab for this event
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public override GameObject GetPrefab(AnimationEventHandler handler, float weight)
        {
            return prefab;
        }
    }
}