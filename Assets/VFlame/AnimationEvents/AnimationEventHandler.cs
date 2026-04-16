using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.AnimationEvents
{
    /// <summary>
    /// Handler for animation events
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationEventHandler : MonoBehaviour
    {
        /// <summary>
        /// Are SFX currently muted
        /// </summary>
        private static bool sfxMuted = false;
        /// <summary>
        /// Set the muted state of SFX for all animation events
        /// </summary>
        public static bool SfxMuted
        {
            get => sfxMuted;
            set
            {
                // No change, don't do any more processing.
                if (sfxMuted == value)
                    return;

                sfxMuted = value;
                onMuteStateChanged.SafeInvoke();
            }
        }
        /// <summary>
        /// Callback for when the muted state changes
        /// </summary>
        private static Action onMuteStateChanged = null;

        /// <summary>
        /// GameObject you can reference for custom implementations of AnimationEventSFX or VFX Parameter Objects to make GetComponent a bit easier.
        /// </summary>
        [Tooltip("GameObject you can reference for custom implementations of AnimationEventSFX or VFX Parameter Objects to make getting your components a bit easier.")]
        public GameObject referenceObject;
        /// <summary>
        /// A monobehaviour you can reference for custom implementations of AnimationEventSFX or VFX Parameter Objects to make getting it a bit easier.
        /// </summary>
        /// <remarks>
        /// You can use GetReferenceBehaviour to get it already cast to the preferred type.
        /// </remarks>
        [SerializeField]
        protected MonoBehaviour referenceBehaviour;

        /// <summary>
        /// The audio sources this event handler can use
        /// </summary>
        public Dictionary<string, AudioSource> audioSources = null;
        /// <summary>
        /// The transforms the VFX events can use.
        /// </summary>
        public Dictionary<string, Transform> parents = null;

        /// <summary>
        /// The animator we are listening too
        /// </summary>
        private Animator animator;
        /// <summary>
        /// The last frame where normalized time was updated.
        /// </summary>
        private float lastNormalizedTime = 0f;

        /// <summary>
        /// The animator this event handler is targeting
        /// </summary>
        public Animator Animator => animator;

        /// <summary>
        /// Get the reference behaviour as a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetReferenceBehaviour<T>() where T : MonoBehaviour
        {
            if (referenceBehaviour == null ||
                referenceBehaviour is not T ret)
                return null;

            return ret;
        }
        /// <summary>
        /// Set the reference behavior to make accessing it easier in custom AnimationEventVFX or SFX parameter objects!
        /// </summary>
        /// <param name="behaviour"></param>
        public void SetReferenceBehaviour(MonoBehaviour behaviour) => referenceBehaviour = behaviour;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
        }

        protected virtual void OnEnable()
        {
            // When re-enabled, recompute normalizedTime. If we keep it as the old value, it could differ greatly and cause
            // blended animation events to fire on the first frame it's re-activated. It'll be like it thinks there was a VERY SLOW frame.
            lastNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;

            // Hook into callback so we can update it if it changes.
            onMuteStateChanged += RefreshMuteState;

            // Make sure audio sources have the correct mute state
            RefreshMuteState();
        }

        protected virtual void OnDisable()
        {   
            // Object is disabled so we probably don't need to update the muted state of audiosources
            // since they are probably also disabled with us.
            // --- I'm sure some poor sole will have a super edge case where an audiosource keeps playing :)
            onMuteStateChanged -= RefreshMuteState;
        }

        /// <summary>
        /// Updates the AudioSources muted state with the event handlers global mute value.
        /// </summary>
        void RefreshMuteState()
        {
            // Make sure the muted state is up to date
            foreach (var audioSource in audioSources.Values)
                if (audioSource)
                    audioSource.mute = SfxMuted;
        }

        /// <summary>
        /// Process blended AnimationEventParameters
        /// </summary>
        protected virtual void LateUpdate()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Not playing so no point processing animation events :P
            if (stateInfo.speed == 0f)
                return;

            // Use pool lists to mitigate memory alloc for getting the clip infos.
            PoolList<AnimatorClipInfo> clipInfos = PoolList<AnimatorClipInfo>.Get(5);
            animator.GetCurrentAnimatorClipInfo(0, clipInfos);

            // Get a buffer object to store our data in as we enumerate over the clips
            var buffer = BufferDictionary.Get<AnimationEventParameter, AnimationEventInfo>(5);
            float normalizedTime;

            // Go over all clips & calculate the blended normalized time for any identical animation events
            foreach (var clipInfo in clipInfos)
                foreach (var animationEvent in clipInfo.clip.events)
                {
                    // Ignore non-parameters & not blended parameters
                    if (animationEvent.objectReferenceParameter is not AnimationEventParameter param ||
                        !param.isBlended)
                        continue;

                    // Compute the normalized time of the event (easier to work with)
                    normalizedTime = animationEvent.time / clipInfo.clip.length;

                    // Get the eventInfo from the buffer. Because AnimationEventInfo is a struct, a default object is exactly what we want if its not in there :D
                    bool contains = buffer.TryGetValue(param, out var eventInfo);
                    eventInfo.weight += clipInfo.weight;

                    if (!contains)
                    {   // First time this event has appeared, use this obejcts normalized time as the baseline for blending calculations.
                        eventInfo.startNormalizedTime = normalizedTime;
                        eventInfo.normalizedTime = normalizedTime;
                    }
                    else
                    {
                        // Calcualte the shortest distance to the animation event
                        float difL = (normalizedTime - 1) - eventInfo.startNormalizedTime;
                        float difR = normalizedTime - eventInfo.startNormalizedTime;
                        float dif = Mathf.Abs(difL) < Mathf.Abs(difR) ? difL : difR;

                        // Compute the weighted normalized time of the event.
                        eventInfo.normalizedTime += dif * clipInfo.weight;
                    }

                    // Shove the updated struct back into the dictionary
                    buffer[param] = eventInfo;
                }

            // Re-use the normalizedTime property to track state normalized time.
            normalizedTime = stateInfo.normalizedTime % 1;

            // If we've got stuff in the buffer
            if (buffer.Count > 0)
            {
                // Create a range to represent the normalizedTime range that has passed. We have 2 pairs of min-max because if it wrapped around
                // we will need a 0-now & prev-1 pair (or vice versa if speed is negative)
                float min1;
                float min2;
                float max1;
                float max2;

                // If speed is > 0, we compute with Rightwards math. Otherwise use Leftwards math. This is important because it changes how we detect when
                // the normalized time has wrapped around.
                if (stateInfo.speed > 0)
                {
                    if (normalizedTime > lastNormalizedTime)
                    {   // No wrap has occurred, east min-max calcs
                        min1 = min2 = lastNormalizedTime;
                        max1 = max2 = normalizedTime;
                    }
                    else
                    {   // A wrap around has occurred. Set group 1 to be from 0-now & group to last-1
                        min1 = 0f;
                        max1 = normalizedTime;
                        min2 = lastNormalizedTime;
                        max2 = 1f;
                    }
                }
                else
                {
                    if (normalizedTime < lastNormalizedTime)
                    {   // No wrap has occurred, east min-max calcs
                        min1 = min2 = normalizedTime;
                        max1 = max2 = lastNormalizedTime;
                    }
                    else
                    {   // A wrap around has occurred. Set group 1 to be from 0-last & group to now-1
                        min1 = 0f;
                        max1 = lastNormalizedTime;
                        min2 = normalizedTime;
                        max2 = 1f;
                    }
                }

                // Check if any events should play
                foreach (var eventAndInfo in buffer)
                {
                    var time = eventAndInfo.Value.NormalizedTime;

                    // Check if the event should be played & play it!
                    if ((time > min1 && time <= max1) ||
                        (time > min2 && time <= max2))
                    {
                        PlayEvent(eventAndInfo.Key, eventAndInfo.Value.weight);
                    }
                }
            }

            // Update last cycles normalized time so we can keep track of event changes.
            lastNormalizedTime = normalizedTime;

            // Done with our pooled data, free them for other systems to use
            clipInfos.Release();
            buffer.Clear();
        }

        /// <summary>
        /// Play an AnimationEventSFXParameter with 1 weighting
        /// </summary>
        /// <param name="obj"></param>
        // Executed via animation callback
        public void PlaySFX(UnityEngine.Object obj)
        {
            if (obj is not AnimationEventSFXParameter sfx)
            {
                Debug.LogError("[VFlame.AnimationEvent] Invalid Parameter for event: " + obj.name, gameObject);
                return;
            }

            // Blended SFX are handled differently & aren't called from animation events directly.
            if (sfx.isBlended)
                return;

            // Default to 1 weighting
            PlaySFX(sfx, 1f);
        }

        /// <summary>
        /// Play an AnimationEventSFXParameter with variable weighting
        /// </summary>
        /// <param name="sfx"></param>
        public virtual void PlaySFX(AnimationEventSFXParameter sfx, float weight)
        {
            // Don't waste time processing SFX if it's all muted.
            if (SfxMuted || sfx == null)
                return;

            var audioSource = sfx.GetAudioSource(this, weight);

            if (audioSource == null)
            {
                Debug.LogError("[VFlame.AnimationEvent] Missing Audio Source for event: " + sfx.name, gameObject);
                return;
            }

            var clip = sfx.GetAudioClip(this, weight);

            if (clip == null)
            {
                Debug.LogError("[VFlame.AnimationEvent] Missing Audio Clip for event: " + sfx.name, gameObject);
                return;
            }

            // Play the audio clip
            audioSource.PlayOneShot(clip, sfx.GetVolume(this, weight));
        }

        /// <summary>
        /// Play an AnimationEventVFXParameter
        /// </summary>
        /// <param name="obj"></param>
        // Executed by animation callback
        public void PlayVFX(UnityEngine.Object obj)
        {
            if (obj is not AnimationEventVFXParameter vfx)
            {
                Debug.LogError("[VFlame.AnimationEvent] Invalid Parameter for event: " + obj.name, gameObject);
                return;
            }

            // Blended VFX are handled differently & aren't called from animation events directly.
            if (vfx.isBlended)
                return;

            // Play assuming 1 wieghting
            PlayVFX(vfx, 1f);
        }

        /// <summary>
        /// Play an AnimationEventVFXParameter
        /// </summary>
        /// <param name="obj"></param>
        public virtual void PlayVFX(AnimationEventVFXParameter vfx, float weight)
        {
            GameObject prefab = vfx.GetPrefab(this, weight);

            // If prefab comes back as null, assume that they don't want to spawn it "this time".
            // Just log a warning (just in case)
            if (prefab == null)
            {
                Debug.LogWarning("[VFlame.AnimationEvent] Missing Prefab for VFX event: " + vfx.name, gameObject);
                return;
            }

            // Spawn the VFX
            Transform parent = vfx.GetParent(this);
            GameObject instance = vfx.GetAttachToParent(this, weight) ?
                Instantiate(prefab, parent) :
                Instantiate(prefab, parent.position, parent.rotation);

            // If the lifespan is above 0, designate the object to die after that time.
            float lifespan = vfx.GetLifespan(this, weight);
            if (lifespan > 0f)
                Destroy(instance, lifespan);
        }

        /// <summary>
        /// Figures out how to properly process an AnimationEventParameter
        /// </summary>
        // Executed by animation callback
        public void PlayEvent(UnityEngine.Object obj)
        {
            if (obj is not AnimationEventParameter param)
            {
                Debug.LogError("[VFlame.AnimationEvent] Invalid Parameter for event: " + obj.name, gameObject);
                return;
            }

            // Handled not through animation callbacks
            if (param.isBlended)
                return;

            // Play with 1 weighting
            PlayEvent(param, 1f);
        }
        /// <summary>
        /// Figures out how to properly process an AnimationEventParameter
        /// </summary>
        /// <param name="param"></param>
        /// <param name="weight"></param>
        public virtual void PlayEvent(AnimationEventParameter param, float weight)
        {
            // To Do: I wonder if there is a more efficent method of doing this. Than attempting each cast 1 by 1.

            if (param is AnimationEventSFXParameter sfx)
            {
                PlaySFX(sfx, weight);
                return;
            }

            if (param is AnimationEventVFXParameter vfx)
            {
                PlayVFX(vfx, weight);
                return;
            }

            if (param is AnimationEventMultiParameter multi)
            {
                multi.InvokeParameters(this, weight);
                return;
            }

            if (param is AnimationEventCustomParameter custom)
            {
                // Execute in try catch as this is a custom implementation. If it's one of mine, then I should fix it. If its theirs, I don't want
                // the rest of the system dying because of it. I know mine won't error :)
                try
                {
                    custom.Invoke(this, weight);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return;
            }

            // Failed to process the event type.
            Debug.LogError("[VFlame.AnimationEvent] Failed to execute Event! No Valid Case for Type!: " + param.name, gameObject);
        }


        /// <summary>
        /// Stores information about when an AnimationEvent should play when blended
        /// </summary>
        public struct AnimationEventInfo
        {
            /// <summary>
            /// The initial normalized time used as a starting point compute blended normalized time
            /// </summary>
            public float startNormalizedTime;
            /// <summary>
            /// The normalized time to play the event at
            /// </summary>
            public float normalizedTime;
            /// <summary>
            /// The weighting of the event
            /// </summary>
            public float weight;

            /// <summary>
            /// The normalized time in 0-1 range
            /// </summary>
            public readonly float NormalizedTime
            {
                get
                {   // Extra handling to make sure normalizedTime stays in 0-1 range.
                    if (normalizedTime < 0)
                        return 1 - normalizedTime;
                    else if (normalizedTime > 1)
                        return normalizedTime - 1;
                    else
                        return normalizedTime;
                }
            }
        }
    }
}