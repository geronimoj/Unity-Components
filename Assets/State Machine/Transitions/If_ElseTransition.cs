using UnityEngine;
using StateMachine.States;
using System.Collections.Generic;

namespace StateMachine.Transitions
{
    /// <summary>
    /// Transition that compares a condition to determine which state to change to. This transition will always pass
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class If_ElseTransition<T> : Transition<T>
    {
        /// <summary>
        /// The state that will be swapped to if the condition succeeds
        /// </summary>
        [SerializeField]
        [Tooltip("The state that will be swapped to if the condition succeeds")]
        private State<T> _ifState = null;
        /// <summary>
        /// The state that will be swaped to if the condition fails
        /// </summary>
        [SerializeField]
        [Tooltip("The state that will be swaped to if the condition fails")]
        private State<T> _elseState = null;

        public override (bool ShouldTransition, IState<T> TargetState) ShouldTransition(T target)
        {   //Check the condition
            if (Condition(ref target))
                return (true, _ifState);
            else
                return (true, _elseState);
        }
        /// <summary>
        /// Checks if the if State should be transitioned to
        /// </summary>
        /// <param name="ctrl">Reference to the object</param>
        /// <returns>Returns true if the ifState should be returned to</returns>
        protected abstract bool Condition(ref T ctrl);

        public override ITransition<T> Initialize(T target, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions)
        {
            var newInstance = (If_ElseTransition< T>)base.Initialize(target, instancedStates, instancedTransitions);

            // Make sure other target states are instanced
            newInstance._ifState = (State<T>)StateManager<T>.GetOrCreateInstance(target, _ifState, instancedStates, instancedTransitions);
            newInstance._elseState = (State<T>)StateManager<T>.GetOrCreateInstance(target, _elseState, instancedStates, instancedTransitions);

            return newInstance;
        }
    }
}