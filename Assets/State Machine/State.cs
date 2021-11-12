using System;
using UnityEngine;
using StateMachine.Transitions;

namespace StateMachine.States
{
    /// <summary>
    /// The base class for all the states
    /// </summary>
    public abstract class State<T> : ScriptableObject
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
        public bool[] ignoreTransition;
        /// <summary>
        /// The initial Start call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void State_Start(ref T c)
        {
            ignoreTransition = new bool[IgnoreTransitions.Length];
            for (int i = 0; i < ignoreTransition.Length; i++)
            {
                ignoreTransition[i] = IgnoreTransitions[i];
            }
            StateStart(ref c);
        }
        /// <summary>
        /// The initial Update call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void State_Update(ref T c)
        {
            StateUpdate(ref c);
        }
        /// <summary>
        /// The initial End call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void State_End(ref T c)
        {
            StateEnd(ref c);
        }
        /// <summary>
        /// The initial call for every Fixed Update. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void State_Fixed(ref T c)
        {
            StateFixedUpdate(ref c);
        }
        /// <summary>
        /// The initial call for every Late Update. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        public void State_Late(ref T c)
        {
            StateLateUpdate(ref c);
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
            for (uint i = 0; i < transitions.Length; i++)
                //Compare type to determine if it should be disabled
                if (transitions[i].GetType() == transition)
                    //Ignore the transition.
                    //We could break here but I'd rather continue for other transitions of same type
                    ignoreTransition[i] = enabled;
        }

    }
}