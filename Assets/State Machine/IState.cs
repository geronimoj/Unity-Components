using System.Collections;
using System.Collections.Generic;
using StateMachine.Transitions;

namespace StateMachine.States
{

    public interface IState<T>
    {
        /// <summary>
        /// Is this state initialized
        /// </summary>
        public bool IsInitialized { get; }

        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="target">The statemachines target object</param>
        /// <param name="instancedStates">A dictionary containing instanced states</param>
        /// <param name="instancedTransitions">A dictionary containing instanced transitions</param>
        /// <returns></returns>
        /// <returns></returns>
        /// <remarks>
        /// Returns a reference to the state incase you want to new your ScriptableObject transitions from a template object. 
        /// 
        /// Use the instanced dictionaries to help resolve cylindical references. Example: When using ScriptableObjects for States and Transitions, you may want to instance each state and transition
        /// before use. The dictionaries can be used to track the original asset -> cloned asset such that cylindrical references can be resolved conveniently.
        /// 
        /// DICTIONARIES ARE TEMPORARY AND WILL BE CLEARED AFTER INITIALIZATION COMPLETES! DO NOT HOLD REFERENCES TO THEM!
        /// </remarks>
        public IState<T> Initialize(T target, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions) => this;

        /// <summary>
        /// Obtains the transitions for the state
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITransition<T>> GetTransitions();

        /// <summary>
        /// Executed when entering the state
        /// </summary>
        /// <param name="target"></param>
        public void OnEnter(T target) { }

        /// <summary>
        /// Executed in Unity's Update function
        /// </summary>
        /// <param name="target"></param>
        public void OnUpdate(T target) { }

        /// <summary>
        /// Executed in Unity's FixedUpdate function
        /// </summary>
        /// <param name="target"></param>
        public void OnFixedUpdate(T target) { }

        /// <summary>
        /// Executed in Unity's LateUpdate function
        /// </summary>
        /// <param name="target"></param>
        public void OnLateUpdate(T target) { }

        /// <summary>
        /// Executed when exiting the state
        /// </summary>
        /// <param name="target"></param>
        public void OnExit(T target) { }
    }

    public static class InstancedStateDictionary
    {
    }
}
