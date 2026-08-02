using System;
using UnityEngine;
using StateMachine.Transitions;
using System.Collections.Generic;

namespace StateMachine.States
{
    /// <summary>
    /// The base class for all the states
    /// </summary>
    public abstract class State<T> : ScriptableObject, IState<T>
    {   /// <summary>
        /// The transitions this state should check
        /// </summary>
        [Tooltip("The transitions for this state")]
        public Transition<T>[] transitions;
        /// <summary>
        /// Any transitions this state should ignore
        /// </summary>
        [SerializeField]
        [Tooltip("The transitions that should be ignored upon entering the state")]
        private bool[] IgnoreTransitions = new bool[0];
        /// <summary>
        /// Any transitions this state should ignore
        /// </summary>
        [HideInInspector]
        internal bool[] ignoreTransition;
        /// <summary>
        /// The list of currently active transitions
        /// </summary>
        List<Transition<T>> activeTransitions = null;

        bool instanced = false;
        public bool IsInitialized => instanced;

        /// <summary>
        /// The initial Start call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void OnEnter(T target)
        {
            ignoreTransition = new bool[IgnoreTransitions.Length];
            for (int i = 0; i < ignoreTransition.Length; i++)
                ignoreTransition[i] = IgnoreTransitions[i];

            StateStart(ref target);
        }
        /// <summary>
        /// The initial Update call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void OnUpdate(T target)
        {
            StateUpdate(ref target);
        }
        /// <summary>
        /// The initial call for every Fixed Update. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void OnFixedUpdate(T target)
        {
            StateFixedUpdate(ref target);
        }
        /// <summary>
        /// The initial call for every Late Update. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void OnLateUpdate(T target)
        {
            StateLateUpdate(ref target);
        }
        /// <summary>
        /// The initial End call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void OnExit(T target)
        {
            StateEnd(ref target);
            ignoreTransition = null;
        }
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        protected virtual void StateStart(ref T obj) { }
        /// <summary>
        /// Called while the state is the current state
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        protected virtual void StateUpdate(ref T obj) { Debug.Log("No State"); }
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        protected virtual void StateEnd(ref T obj) { }
        /// <summary>
        /// Called while the state is the current state
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        protected virtual void StateFixedUpdate(ref T obj) { }
        /// <summary>
        /// Called while the state is the current state
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        protected virtual void StateLateUpdate(ref T obj) { }
        /// <summary>
        /// Re-enabled all transitions
        /// </summary>
        protected void ReEnableTransitions()
        {
            for (int i = 0; i < ignoreTransition.Length; i++)
                ignoreTransition[i] = false;
        }
        /// <summary>
        /// Toggles the state of a transition
        /// </summary>
        /// <param name="transition">The type of transition to change the state of</param>
        /// <param name="enabled">The on/off state to set the transition to</param>
        public void ToggleTransition(Type transition, bool enabled)
        {   //Loop over the transitions
            for (int i = 0; i < transitions.Length; i++)
                //Compare type to determine if it should be disabled
                if (transitions[i].GetType() == transition)
                    //Ignore the transition.
                    //We could break here but I'd rather continue for other transitions of same type
                    ignoreTransition[i] = enabled;
        }
        /// <summary>
        /// Returns the ignored state of the transition (true if the transition is being ignored)
        /// False is the transition does not exist on the state
        /// </summary>
        /// <param name="transition"></param>
        /// <returns></returns>
        public bool TransitionEnabled(Type transition)
        {   //Loop over the transitions
            for (int i = 0; i < transitions.Length; i++)
                //Compare type to determine if it should be disabled
                if (transitions[i].GetType() == transition)
                    //Ignore the transition.
                    //We could break here but I'd rather continue for other transitions of same type
                    return ignoreTransition[i];
            return false; // Doesn't exist
        }

        public virtual IState<T> Initialize(T target, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions)
        {
            // Create a new instance and flag it as instanced
            var newInstance = ScriptableObject.Instantiate(this);
            newInstance.instanced = true;

            // Track us in the dictionary to resolve cylindircal references
            instancedStates[this] = newInstance;

            // Instance any transitions on this state
            for (int i = 0; i < transitions.Length; i++)
            {
                newInstance.transitions[i] = (Transition<T>)StateManager<T>.GetOrCreateInstance(target, transitions[i], instancedStates, instancedTransitions);
            }

            return newInstance;
        }

        public virtual IEnumerable<ITransition<T>> GetTransitions()
        {
            activeTransitions ??= new List<Transition<T>>(transitions.Length);
            activeTransitions.Clear();

            // Check which transitions are active and add them to the list
            for (int i = 0; i < transitions.Length; i++)
            {
                if (!ignoreTransition[i])
                    activeTransitions.Add(transitions[i]);
            }

            return activeTransitions;
        }
    }
}