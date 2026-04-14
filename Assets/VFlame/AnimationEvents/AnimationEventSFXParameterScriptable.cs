using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Animation event parameter for SFX
    /// </summary>
    [CreateAssetMenu(fileName = "Event_NAME_SFX_Parameters", menuName = "VFlame/Animation/Events/SFX")]
    public class AnimationEventSFXParameterScriptable : AnimationEventSFXParameter
    {
        /// <summary>
        /// Unique Id of the audio source to play the SFX from
        /// </summary>
        protected string audioSourceId;
        /// <summary>
        /// The volume to play the Sound effect at
        /// </summary>
        protected float volume;
        /// <summary>
        /// The audio clip to play
        /// </summary>
        protected AudioClip clip;

        /// <summary>
        /// Get the audio source to play the SFX from
        /// </summary>
        /// <param name="handler">The animation event handler processing</param>
        /// <returns></returns>
        public override AudioSource GetAudioSource(AnimationEventHandler handler, float weight)
        {
            if (handler.audioSources.TryGetValue(audioSourceId, out var audioSource))
                return audioSource;

            return null;
        }
        /// <summary>
        /// Get the volume of the clip to play
        /// </summary>
        /// <param name="handler">The animation event handler processing</param>
        /// <param name="clipWeight">The weight of the animation event in blending</param>
        /// <returns></returns>
        public override float GetVolume(AnimationEventHandler handler, float weight)
        {
            return volume * weight;
        }
        /// <summary>
        /// Get the clip to play
        /// </summary>
        /// <param name="handler">The animation event handler processing</param>
        /// <returns></returns>
        public override AudioClip GetAudioClip(AnimationEventHandler handler, float weight)
        {
            return clip;
        }
    }
}
