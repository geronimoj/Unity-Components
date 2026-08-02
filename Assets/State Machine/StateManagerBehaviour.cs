using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;
using StateMachine.Transitions;
using UnityEngine.Serialization;

namespace StateMachine
{
    /// <summary>
    /// Manages the states. Checks for transitions, swaps between states & calls update on the current state.
    /// State Update, FixedUpdate and Late Update have to be manually called by calling DoState, DoFixedUpdate and DoLateUpdate but
    /// this can be automated by defining AUTO_STATE_MACHINE but it will required _target to be assigned through SetTarget
    /// </summary>
    public class StateManagerBehaviour<T> : MonoBehaviour
    {
        /// <summary>
        /// The manager for the stateManager
        /// </summary>
        private StateManager<T> _manager;

        /// <summary>
        /// The state to start the state manager in
        /// </summary>
        [SerializeField, FormerlySerializedAs("_current")]
        private State<T> _initialState;

        /// <summary>
        /// These transitions will always be checked reguardless as to what state we are currently in
        /// </summary>
        public Transition<T>[] globalTransitions = new Transition<T>[0];

        /// <summary>
        /// Should transitions be checked in Update?
        /// </summary>
        public bool processTransitionsInUpdate = true;
        /// <summary>
        /// Should transitions be checked in FixedUpdate?
        /// </summary>
        public bool processTransitionsInFixedUpdate = false;
        /// <summary>
        /// Should transitions be checked in LateUpdate?
        /// </summary>
        public bool processTransitionsInLateUpdate = false;

        /// <summary>
        /// The state manager
        /// </summary>
        public StateManager<T> Manager => _manager;

        /// <summary>
        /// Initializes the state manager with its initial state or a specific state
        /// </summary>
        /// <param name="target"></param>
        /// <param name="initialState"></param>
        public void Initialize(T target, State<T> initialState = null)
        {
            if (initialState == null)
                initialState = _initialState;

            _manager ??= new StateManager<T>();
            _manager.Initialize(target, initialState);
            _manager.AnyStateTransitions = globalTransitions;
        }

        /// <summary>
        /// Sets a new target for the state manager
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(T target)
        {
            _manager?.SetTarget(target);
        }

        /// <summary>
        /// Sets the target state
        /// </summary>
        /// <param name="targetState"></param>
        public void SetState(State<T> targetState)
        {
            _manager?.SetState(targetState);
        }

        /// <summary>
        /// Manually call to process the transitions
        /// </summary>
        public void ProcessTransitions()
        {
            _manager?.ProcessTransitions();
        }

        /// <summary>
        /// Then updates the current state if we have one. Also checks transitions if processTransitionsInUpdate is true
        /// </summary>
        public void ProcessUpdate()
        {
            // Process the transitions then execute update
            if (processTransitionsInUpdate)
                _manager?.ProcessTransitions();
            _manager?.ProcessUpdate();
        }

        /// <summary>
        /// Performs the fixed update loop for the State. Also checks transitions if processTransitionsInFixedUpdate is true
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        public void ProcessFixedUpdate()
        {
            if (processTransitionsInFixedUpdate)
                _manager?.ProcessTransitions();
            _manager?.ProcessFixedUpdate();
        }
        /// <summary>
        /// Performs the late update loop for the State. Also checks transitions if processTransitionsInLateUpdate is true
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        public void DoLateUpdate()
        {
            if (processTransitionsInLateUpdate)
                _manager?.ProcessTransitions();
            _manager?.ProcessLateUpdate();
        }

#if AUTO_STATE_MACHINE
        protected virtual void Update() => ProcessUpdate();

        protected virtual void FixedUpdate() => ProcessFixedUpdate();

        protected virtual void LateUpdate() => DoLateUpdate();
#endif
    }
}