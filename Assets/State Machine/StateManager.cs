using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;
using StateMachine.Transitions;
using System;

namespace StateMachine
{
    public class StateManager<T>
    {
        /// <summary>
        /// The StateManagers target object
        /// </summary>
        public T Target { get; private set; }

        /// <summary>
        /// The Current State
        /// </summary>
        public IState<T> CurrentState { get; private set; }

        /// <summary>
        /// An enumerable collection of transitions that apply to all states (processed before State transitions)
        /// </summary>
        public IEnumerable<ITransition<T>> AnyStateTransitions { get; set; }

        /// <summary>
        /// Callback made after the state changes (Old State, New State)
        /// </summary>
        public event Action<IState<T>, IState<T>> OnStateChanged
        {
            add
            {
                onStateChanged += value;
            }
            remove
            {
                onStateChanged -= value;
            }
        }

        /// <summary>
        /// Callback made after the state changes (Old State, New State)
        /// </summary>
        protected Action<IState<T>, IState<T>> onStateChanged;

        /// <summary>
        /// Initialize the StateManager
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="initialState"></param>
        public virtual void Initialize(T obj, IState<T> initialState)
        {
            Target = obj;
            SetState(initialState);
        }

        /// <summary>
        /// Change the target of the state manager
        /// </summary>
        /// <param name="obj"></param>
        public virtual void SetTarget(T obj)
        {
            Target = obj;
        }

        /// <summary>
        /// Set the current state to a specific state
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newState"></param>
        public void SetState(IState<T> newState)
        {
            // If the target state is not initialized, initialize it before transitioning
            if (!newState.IsInitialized)
            {
                // Allocate pool dictionaries for tracking asset -> instanced states (in case users use ScriptableObjects or Templates)
                PoolDictionary<IState<T>, IState<T>> instancedStates = PoolDictionary<IState<T>, IState<T>>.Get(30);
                PoolDictionary<ITransition<T>, ITransition<T>> instancedTransitions = PoolDictionary<ITransition<T>, ITransition<T>>.Get(30);

                // Initialize the newState, passing in the temporary dictionaries to help with instancing.
                newState = newState.Initialize(Target, instancedStates, instancedTransitions);

                // Release the dictionaries back to the pool so they can be re-used
                instancedStates.Release();
                instancedTransitions.Release();
            }

            var oldState = CurrentState;

            // Swap the states around
            CurrentState?.OnExit(Target);
            CurrentState = newState;
            CurrentState?.OnEnter(Target);

            // Invoke the state changed callback
            onStateChanged.SafeInvoke(oldState, newState);
        }

        /// <summary>
        /// Process the transitions for the current state
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool ProcessTransitions()
        {
            // Process the any state transitions, then current state.
            // If any of the AnyStateTransitions are applied, it won't process CurrentState due to order of operations.
            return Process(AnyStateTransitions) || Process(CurrentState?.GetTransitions());

            bool Process(IEnumerable<ITransition<T>> transitions)
            {
                // Process the transitions, the first one to return true will be transitioned too
                if (transitions != null)
                    foreach (var transition in transitions)
                    {
                        (bool changeState, IState<T> target) = transition.ShouldTransition(Target);

                        // If this state has changed, swap the current state.
                        if (changeState)
                        {
                            SetState(target);
                            return true;
                        }
                    }

                return false;
            }
        }

        public virtual void ProcessUpdate()
        {
            CurrentState?.OnUpdate(Target);
        }

        public virtual void ProcessFixedUpdate()
        {
            CurrentState?.OnFixedUpdate(Target);
        }

        public virtual void ProcessLateUpdate()
        {
            CurrentState?.OnLateUpdate(Target);
        }

        public static IState<T> GetOrCreateInstance(T target, IState<T> targetState, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions)
        {
            // If null or already initailized, don't waste time on gets, just return the initailized state
            if (targetState == null || targetState.IsInitialized)
                return targetState;

            // If the target state is already instanced, grab it from the instanced list.
            if (instancedStates.TryGetValue(targetState, out var newTarget))
            {
                return newTarget;
            }
            else
            {   // Otherwise Initialize a new instance
                return targetState.Initialize(target, instancedStates, instancedTransitions);
            }
        }
        public static ITransition<T> GetOrCreateInstance(T target, ITransition<T> targetTransition, Dictionary<IState<T>, IState<T>> instancedStates, Dictionary<ITransition<T>, ITransition<T>> instancedTransitions)
        {
            // If null or already initailized, don't waste time on gets, just return the initailized transition
            if (targetTransition == null || targetTransition.IsInitialized)
                return targetTransition;

            // If the target transition is already instanced, grab it from the instanced list.
            if (instancedTransitions.TryGetValue(targetTransition, out var newTarget))
            {
                return newTarget;
            }
            else
            {   // Otherwise Initialize a new instance
                return targetTransition.Initialize(target, instancedStates, instancedTransitions);
            }
        }
    }
}