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
        internal bool[] ignoreTransition;
        /// <summary>
        /// The initial Start call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        internal void State_Start(ref T c)
        {
            ignoreTransition = new bool[IgnoreTransitions.Length];
            for (int i = 0; i < ignoreTransition.Length; i++)
                ignoreTransition[i] = IgnoreTransitions[i];

            StateStart(ref c);
        }
        /// <summary>
        /// The initial Update call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        internal void State_Update(ref T c)
        {
            StateUpdate(ref c);
        }
        /// <summary>
        /// The initial End call. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        internal void State_End(ref T c)
        {
            StateEnd(ref c);
            ignoreTransition = null;
        }
        /// <summary>
        /// The initial call for every Fixed Update. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        internal void State_Fixed(ref T c)
        {
            StateFixedUpdate(ref c);
        }
        /// <summary>
        /// The initial call for every Late Update. For anything that needs to be called globally across all states
        /// </summary>
        /// <param name="c">A reference to the object</param>
        internal void State_Late(ref T c)
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
        /// <summary>
        /// Returns a clone of the current state
        /// </summary>
        /// <returns></returns>
        internal State<T> Clone()
        {
            State<T> ret;
            //Check if already cloned
            if (StateManager<T>.temp_clonedStated.ContainsKey(this))
                ret = StateManager<T>.temp_clonedStated[this];
            else
            {   //Create new clone
                ret = Instantiate(this);
                StateManager<T>.temp_clonedStated.Add(this, ret);
                InternalClone(ret);
                //Clone transitions
                for (int i = 0; i < transitions.Length; i++)
                    if (transitions[i])
                        transitions[i] = transitions[i].Clone();
            }

            return ret;
        }
        /// <summary>
        /// Overridable function for doing any additional clone behaviour that Instantiate would not perform.
        /// </summary>
        /// <param name="cloneInstance"></param>
        protected virtual void InternalClone(State<T> cloneInstance) { }
    }
}