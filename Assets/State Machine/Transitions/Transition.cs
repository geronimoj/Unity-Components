using UnityEngine;
using StateMachine.States;

namespace StateMachine.Transitions
{
    /// <summary>
    /// The base transition. All transitions inherit from this class
    /// </summary>
    public abstract class Transition<T> : ScriptableObject
    {
        /// <summary>
        /// The state we should transition to if this transition returns true
        /// </summary>
        public State<T> targetState = null;
        /// <summary>
        /// An overridable function for creating transitions
        /// </summary>
        /// <param name="ctrl">A reference to the player controller</param>
        /// <returns>Returns false by default</returns>
        public abstract bool ShouldTransition(ref T ctrl);
        /// <summary>
        /// Returns a clone of the current transition
        /// </summary>
        /// <returns></returns>
        internal Transition<T> Clone()
        {
            Transition<T> ret;
            //Check if already cloned
            if (StateManager<T>.temp_clonedTransitions.ContainsKey(this))
                ret = StateManager<T>.temp_clonedTransitions[this];
            else
            {   //Not cloned so clone
                ret = Instantiate(this);
                StateManager<T>.temp_clonedTransitions.Add(this, ret);
                InternalClone(ret);
                //Clone target state
                if (targetState)
                    targetState = targetState.Clone();
            }

            return ret;
        }
        /// <summary>
        /// Overridable function for doing any additional clone behaviour that Instantiate would not perform.
        /// </summary>
        /// <param name="cloneInstance"></param>
        protected virtual void InternalClone(Transition<T> cloneInstance) { }
    }
}