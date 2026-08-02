using StateMachine.States;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.Transitions
{
    /// <summary>
    /// The base transition. All transitions inherit from this class
    /// </summary>
    public abstract class Transition<T> : ScriptableObject, ITransition<T>
    {
        /// <summary>
        /// The state we should transition to if this transition returns true
        /// </summary>
        public State<T> targetState = null;

        /// <summary>
        /// His this object been instanced
        /// </summary>
        bool instanced = false;

        /// <summary>
        /// Has the scriptableObject been instanced at runtime.
        /// </summary>
        public bool IsInitialized => instanced;

        public abstract (bool ShouldTransition, IState<T> TargetState) ShouldTransition(T target);

        [Obsolete("Implement from ShouldTransition that returns (bool, State) instead. It's more efficient")]
        public virtual bool ShouldTransition(ref T target) => false;

        public virtual ITransition<T> Initialize(T target, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions)
        {
            // Create a new instance and flag it as instanced
            var newInstance = ScriptableObject.Instantiate(this);
            newInstance.instanced = true;

            // Track us in the dictionary to resolve cylindircal references
            instancedTransitions[this] = newInstance;

            // Instance the target state
            newInstance.targetState = (State<T>)StateManager<T>.GetOrCreateInstance(target, targetState, instancedStates, instancedTransitions);

            // Return the newly created instance. This should only occur on un-instanced transitions.
            return newInstance;
        }
    }
}