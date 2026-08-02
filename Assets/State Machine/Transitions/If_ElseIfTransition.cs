using UnityEngine;
using StateMachine.States;
using System.Collections.Generic;

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

        public override (bool ShouldTransition, IState<T> TargetState) ShouldTransition(T target)
        {//Perform any universal checks that both IfCondition and elseIfCondition may want
            if (!UniversalChecks(ref target))
                return (false, null);
            //Check if condition
            if (IfCondition(ref target))
            {
                return (true, _ifState);
            }
            //Check elseIf condition
            else if (IfElseCondition(ref target))
            {
                return (true, _elseIfState);
            }
            //Return false
            return (false, null);
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

        public override ITransition<T> Initialize(T target, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions)
        {
            var newInstance = (If_ElseIfTransition<T>)base.Initialize(target, instancedStates, instancedTransitions);

            // Make sure other target states are instanced
            newInstance._ifState = (State<T>)StateManager<T>.GetOrCreateInstance(target, _ifState, instancedStates, instancedTransitions);
            newInstance._elseIfState = (State<T>)StateManager<T>.GetOrCreateInstance(target, _elseIfState, instancedStates, instancedTransitions);

            return newInstance;
        }
    }
}