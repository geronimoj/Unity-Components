using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulator
{
    /// <summary>
    /// Contains function for simulating animator
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [System.Obsolete("You can't do this :(")]   //-----------------------------------This is not possible due to how Unity controls accessability of the animators
    public class AnimatorSimulator : MonoBehaviour
    {
        /// <summary>
        /// The animator to simulate on
        /// </summary>
        Animator _animator;
        /// <summary>
        /// The size of each step during simulation
        /// </summary>
        [Range(0.0001f, 1f)]
        [Tooltip("Size of each step during simulation")]
        [SerializeField] float _step = 0.01f;
        /// <summary>
        /// Gets animator
        /// </summary>
        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        /// <summary>
        /// Simulates the animator for the given duration
        /// </summary>
        /// <param name="duration">Duration to simulate for</param>
        public void Simulate(float duration) => _animator.Simulate(duration, _step);
        /// <summary>
        /// Simulates the animator for the given duration
        /// </summary>
        /// <param name="animator">Animator to simulate</param>
        /// <param name="duration">Duration to simulate for</param>
        public static void SimulateAnimator(Animator animator, float duration, float step = 0.01f) => animator.Simulate(duration, step);
    }
    /// <summary>
    /// Container class for storing the animators extention method for simulation
    /// </summary>
    public static class AnimSimulator
    {
        /// <summary>
        /// Simulates the animator for the given duration
        /// </summary>
        /// <param name="animator">Animator to simulate</param>
        /// <param name="duration">Duration to simulate for</param>
        public static void Simulate(this Animator animator, float duration, float step = 0.01f)
        {
            AnimatorStateInfo[] states;

                int layers = animator.layerCount;
                //If only we had Span<T> in Unity, could cut out heap allocation
                states = new AnimatorStateInfo[layers];

                //Gather state info
                for (int i = 0; i < layers; i++)
                    states[i] = animator.GetCurrentAnimatorStateInfo(i);

            while (duration > 0f)
            {   //Loop over states
                for (int i = 0; i < layers; i++)
                {
                    AnimatorStateInfo state = states[i];
                }

                duration -= step;
            }
        }
    }
}