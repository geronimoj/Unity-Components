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
        public State<T> targetState;
        /// <summary>
        /// An overridable function for creating transitions
        /// </summary>
        /// <param name="ctrl">A reference to the player controller</param>
        /// <returns>Returns false by default</returns>
        public abstract bool ShouldTransition(ref T ctrl);
    }
}