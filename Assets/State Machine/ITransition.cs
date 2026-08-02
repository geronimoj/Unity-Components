using System.Collections;
using System.Collections.Generic;
using StateMachine.States;

namespace StateMachine.Transitions
{
    public interface ITransition<T>
    {
        /// <summary>
        /// Is this state initialized
        /// </summary>
        public bool IsInitialized { get; }

        /// <summary>
        /// Initialize the transition
        /// </summary>
        /// <param name="target">The statemachines target object</param>
        /// <param name="instancedStates">A dictionary containing instanced states</param>
        /// <param name="instancedTransitions">A dictionary containing instanced transitions</param>
        /// <returns></returns>
        /// <returns></returns>
        /// <remarks>
        /// Returns a reference to the transition incase you want to new your ScriptableObject transitions from a template object.
        /// 
        /// Use the instanced dictionaries to help resolve cylindical references. Example: When using ScriptableObjects for States and Transitions, you may want to instance each state and transition
        /// before use. The dictionaries can be used to track the original asset -> cloned asset such that cylindrical references can be resolved conveniently.
        /// 
        /// DICTIONARIES ARE TEMPORARY AND WILL BE CLEARED AFTER INITIALIZATION COMPLETES! DO NOT HOLD REFERENCES TO THEM!
        /// </remarks>
        public ITransition<T> Initialize(T target, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions) => this;

        /// <summary>
        /// Should the transition change state
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Should the transition occur and what is the target state to transition to</returns>
        public (bool ShouldTransition, IState<T> TargetState) ShouldTransition(T target);
    }
}