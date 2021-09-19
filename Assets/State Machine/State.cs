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
        public Transition<T>[] transitions;
        /// <summary>
        /// Any transitions this state should ignore
        /// </summary>
        public bool[] IgnoreTransitions;
        /// <summary>
        /// Any transitions this state should ignore
        /// </summary>
        [HideInInspector]
        public bool[] ignoreTransition;
        /// <summary>
        /// The initial Start call. For anything that needs to be called globally across all transitions
        /// </summary>
        /// <param name="c">A reference to the player controller</param>
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
        /// The initial Update call. For anything that needs to be called globally across all transitions
        /// </summary>
        /// <param name="c">A reference to the player controller</param>
        public void State_Update(ref T c)
        {
            StateUpdate(ref c);
        }
        /// <summary>
        /// The initial End call. For anything that needs to be called globally across all transitions
        /// </summary>
        /// <param name="c">A reference to the player controller</param>
        public void State_End(ref T c)
        {
            StateEnd(ref c);
        }
        /// <summary>
        /// Called when the state is entered
        /// </summary>
        /// <param name="ctrl">A reference to the player controller</param>
        protected virtual void StateStart(ref T ctrl) { }
        /// <summary>
        /// Called while the state is the current state
        /// </summary>
        /// <param name="ctrl">A reference to the player controller</param>
        protected virtual void StateUpdate(ref T ctrl) { Debug.Log("No State"); }
        /// <summary>
        /// Called when the state is exited
        /// </summary>
        /// <param name="ctrl">A reference to the player controller</param>
        protected virtual void StateEnd(ref T ctrl) { }

        protected void ReEnableTransitions()
        {
            for (int i = 0; i < ignoreTransition.Length; i++)
                ignoreTransition[i] = false;
        }
    }
}