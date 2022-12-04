using UnityEngine;
using StateMachine.States;

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
        public override bool ShouldTransition(ref T ctrl)
        {   //Check the condition
            if (Condition(ref ctrl))
                targetState = _ifState;
            else
                targetState = _elseState;

            return true;
        }
        /// <summary>
        /// Checks if the if State should be transitioned to
        /// </summary>
        /// <param name="ctrl">Reference to the object</param>
        /// <returns>Returns true if the ifState should be returned to</returns>
        protected abstract bool Condition(ref T ctrl);
        /// <summary>
        /// Clones the IF state & ELSE state
        /// </summary>
        /// <param name="cloneInstance"></param>
        protected override void InternalClone(Transition<T> cloneInstance)
        {   //Clone the If else states
            If_ElseTransition<T> _this = (If_ElseTransition<T>)cloneInstance;

            _this._ifState = _ifState.Clone();
            _this._elseState = _elseState.Clone();
        }
    }
}