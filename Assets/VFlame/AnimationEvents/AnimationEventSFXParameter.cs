using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Base Animation event parameter for SFX
    /// </summary>
    public abstract class AnimationEventSFXParameter : AnimationEventParameter
    {
        /// <summary>
        /// Get the audio source to play the SFX from
        /// </summary>
        /// <param name="handler">The animation event handler processing</param>
        /// <returns></returns>
        public abstract AudioSource GetAudioSource(AnimationEventHandler handler, float weight);
        /// <summary>
        /// Get the volume of the clip to play
        /// </summary>
        /// <param name="handler">The animation event handler processing</param>
        /// <param name="clipWeight">The weight of the animation event in blending</param>
        /// <returns></returns>
        public abstract float GetVolume(AnimationEventHandler handler, float weight);
        /// <summary>
        /// Get the clip to play
        /// </summary>
        /// <param name="handler">The animation event handler processing</param>
        /// <returns></returns>
        public abstract AudioClip GetAudioClip(AnimationEventHandler handler, float weight);
    }
}