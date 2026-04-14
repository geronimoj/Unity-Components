// Create by Luke Jones 14/04/2026
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Base class for animation event parameter objects
    /// </summary>
    public abstract class AnimationEventParameter : ScriptableObject
    {
        /// <summary>
        /// Should this event blend with identical events when blending animation clips
        /// </summary>
        public bool isBlended;
    }
}