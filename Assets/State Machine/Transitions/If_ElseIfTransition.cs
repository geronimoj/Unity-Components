using UnityEngine;
using StateMachine.States;

namespace StateMachine.Transitions
{
    /// <summary>
    /// A transition for a If_ElseIf transition requirements
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    public abstract class If_ElseIfTransition<T> : Transition<T>
    {
        /// <summary>
        /// The state to swap to if the if passes
        /// </summary>
        [SerializeField]
        [Tooltip("The state to swap to if the first condition passes")]
        private State<T> _ifState = null;
        /// <summary>
        /// The state to swap to if the elseIf passes
        /// </summary>
        [SerializeField]
        [Tooltip("The state to swap to if the second condition passes")]
        private State<T> _elseIfState = null;

        public override bool ShouldTransition(ref T ctrl)
        {   //Perform any universal checks that both IfCondition and elseIfCondition may want
            if (!UniversalChecks(ref ctrl))
                return false;
            //Check if condition
            if (IfCondition(ref ctrl))
            {
                targetState = _ifState;
                return true;
            }
            //Check elseIf condition
            else if (IfElseCondition(ref ctrl))
            {
                targetState = _elseIfState;
                return true;
            }
            //Return false
            return false;
        }
        /// <summary>
        /// Perform any checks / changes necessary before the other conditions get called
        /// </summary>
        /// <param name="c">A reference to the player controller</param>
        /// <returns>Returns true if you want to do the conditional checks. Returning false does not change the state</returns>
        protected virtual bool UniversalChecks(ref T c) { return true; }
        /// <summary>
        /// The check for the ifState
        /// </summary>
        /// <param name="c">A reference to the player controller</param>
        /// <returns>Returns true if the condition is met</returns>
        protected abstract bool IfCondition(ref T c);

        /// <summary>
        /// The check for the elseIfState
        /// </summary>
        /// <param name="c">A reference to the player controller</param>
        /// <returns>Returns true if the condition is met</returns>
        protected abstract bool IfElseCondition(ref T c);
        /// <summary>
        /// Clones the IF state & ELSE IF state
        /// </summary>
        /// <param name="cloneInstance"></param>
        protected override void InternalClone(Transition<T> cloneInstance)
        {   //Clone the If else states
            If_ElseIfTransition<T> _this = (If_ElseIfTransition<T>)cloneInstance;

            _this._ifState = _ifState.Clone();
            _this._elseIfState = _elseIfState.Clone();
        }
    }
}