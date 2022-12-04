using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;
using StateMachine.Transitions;

namespace StateMachine
{
    /// <summary>
    /// Manages the states. Checks for transitions, swaps between states & calls update on the current state.
    /// State Update, FixedUpdate and Late Update have to be manually called by calling DoState, DoFixedUpdate and DoLateUpdate but
    /// this can be automated by defining AUTO_STATE_MACHINE but it will required _target to be assigned through SetTarget
    /// </summary>
    public class StateManager<T> : MonoBehaviour
    {
#if PREVIOUS_STATE_MACHINE
        /// <summary>
        /// A reference to the previous state in case we want to return to it
        /// </summary>
        [SerializeField]
        [Tooltip("The previous state. For help debugging.")]
        private State<T> _previous = null;
        /// <summary>
        /// Reference to the previous active state
        /// </summary>
        public State<T> Previous => _previous;
#endif
        /// <summary>
        /// To clone the current state before first execution
        /// </summary>
        [SerializeField]
        [Tooltip("To clone the current state & transitions (and their states, etc) before its first execution")]
        private bool cloneBeforeExecution = false;
        /// <summary>
        /// The current state
        /// </summary>
        [SerializeField]
        private State<T> _current = null;
        /// <summary>
        /// The current state
        /// </summary>
        public State<T> Current => _current;
        /// <summary>
        /// The state we want to swap to. This is only public for debugging purposes
        /// </summary>
        [SerializeField]
        private State<T> _target = null;
        /// <summary>
        /// These transitions will always be checked reguardless as to what state we are currently in
        /// </summary>
        public Transition<T>[] globalTransitions = new Transition<T>[0];

#if AUTO_STATE_MACHINE
        /// <summary>
        /// The target for this state machine.
        /// </summary>
        [SerializeField]
        private T _targetObj = default;
#endif
        /// <summary>
        /// Checks the transitions, swaps the current state if any return true.
        /// Then updates the current state if we have one
        /// </summary>
        public void DoState(ref T obj)
        {   //Don't do anything if we don't have a reference to the controller
            if (EqualityComparer<T>.Default.Equals(obj, default))
                return;
            //Represents the start function
            if (_current == null && _target != null)
            {
                _current = _target;

                if (cloneBeforeExecution)
                    CloneStatesAndTransitions();

                _current.State_Start(ref obj);
            }
            //Make sure we have a state we can call
            if (_current != null)
            {
                //Do we need to make a global transition                    //Do we need to make a transition out of the current state
                if (CheckTransitions(ref obj, ref globalTransitions, null) || CheckTransitions(ref obj, ref _current.transitions, _current.ignoreTransition))
                    //Swap the states
                    SwapStates(ref obj);
                //Call update on our current state
                _current.State_Update(ref obj);
            }
        }
        /// <summary>
        /// Performs the fixed update loop for the State. Also checks transitions if FIXED_CHECK_TRANSITIONS is defined
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        public void DoFixedUpdate(ref T obj)
        {   //Null catch/default catch. This is kind of redundant for structs but its necessary to avoid having to default/null catch in every state
            if (EqualityComparer<T>.Default.Equals(obj, default))
                return;
            //Null catch again
            if (_current != null)
            {   //Defines woo
#if FIXED_CHECK_TRANSITIONS
                //Do we need to make a global transition                    //Do we need to make a transition out of the current state
                if (CheckTransitions(ref ctrl, ref globalTransitions, null) || CheckTransitions(ref ctrl, ref current.transitions, current.ignoreTransition))
                    //Swap the states
                    SwapStates(ref ctrl);
#endif
                //Call the fixed update
                _current.State_Fixed(ref obj);
            }
        }
        /// <summary>
        /// Performs the late update loop for the State. Also checks transitions if LATE_CHECK_TRANSITIONS is defined
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        public void DoLateUpdate(ref T obj)
        {   //Null catch/default catch. This is kind of redundant for structs but its necessary to avoid having to default/null catch in every state
            if (EqualityComparer<T>.Default.Equals(obj, default))
                return;
            //Null catch again
            if (_current != null)
            {   //Defines woo
#if LATE_CHECK_TRANSITIONS
                //Do we need to make a global transition                    //Do we need to make a transition out of the current state
                if (CheckTransitions(ref ctrl, ref globalTransitions, null) || CheckTransitions(ref ctrl, ref current.transitions, current.ignoreTransition))
                    //Swap the states
                    SwapStates(ref ctrl);
#endif
                //Call the fixed update
                _current.State_Late(ref obj);
            }
        }

#if AUTO_STATE_MACHINE
        /// <summary>
        /// Sets the target for the State Machine
        /// </summary>
        /// <param name="target">The target object</param>
        public void SetTarget(T target)
        {   //Set the target
            _targetObj = target;
        }

        private void Update()
        {   //Automatically call the state
            DoState(ref _targetObj);
        }

        private void FixedUpdate()
        {   //Automatically call the state
            DoFixedUpdate(ref _targetObj);
        }

        private void LateUpdate()
        {   //Automatically call the state
            DoLateUpdate(ref _targetObj);
        }
#endif
        /// <summary>
        /// Checks the transitions
        /// </summary>
        /// <param name="ctrl">A reference to the player controller</param>
        /// <param name="trans">The transitions that should be checked</param>
        /// <returns>Returns true on the first transition to return true</returns>
        bool CheckTransitions(ref T ctrl, ref Transition<T>[] trans, bool[] ignore)
        {   //Make sure we have transitions to check
            if (trans == null)
                return false;
            for (int i = 0; i < trans.Length; i++)
            {   //Should this transition be ignored
                if (ignore != null && i < ignore.Length)
                    if (ignore[i])
                        continue;
                //Should we transition
                if (trans[i].ShouldTransition(ref ctrl))
                {
                    //Swap target and return true
                    _target = trans[i].targetState;
                    return true;
                }
            }
            //No transitions passed
            return false;
        }
        /// <summary>
        /// Swaps target state to current state
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        void SwapStates(ref T obj)
        {   //Make sure we have a valid controller
            if (EqualityComparer<T>.Default.Equals(obj, default) || _target == _current || _target == null)
                return;
            //Call end on our current state
            _current.State_End(ref obj);
            //Swap our states around
#if PREVIOUS_STATE_MACHINE
            _previous = _current;
#endif
            _current = _target;
            _target = null;
            //call state on our new state
            _current.State_Start(ref obj);
        }
        /// <summary>
        /// Forces the state to swap to the specified state immediately
        /// </summary>
        /// <param name="obj">A reference to the object</param>
        /// <param name="targetState">The target state</param>
        public void ForceSwapStates(ref T obj, State<T> targetState)
        {   //Set the target
            _target = targetState;
            //Swap the state
            SwapStates(ref obj);
        }

        #region Cloning
        /// <summary>
        /// Temporary storage for cloned states to avoid re-cloning the same state
        /// </summary>
        internal static Dictionary<State<T>, State<T>> temp_clonedStated = null;
        /// <summary>
        /// Temporary storage for cloned transitions to avoid re-cloning the same transition
        /// </summary>
        internal static Dictionary<Transition<T>, Transition<T>> temp_clonedTransitions = null;
        /// <summary>
        /// Clones the currently used states & transitions then starts using them.
        /// </summary>
        public void CloneStatesAndTransitions()
        {   //Must contain current
            if (!_current)
                return;
            //Allocate storage for tracking cloned states (only needed temporary but the states & transitions need access to them.
            temp_clonedStated = new Dictionary<State<T>, State<T>>();
            temp_clonedTransitions = new Dictionary<Transition<T>, Transition<T>>();

            //Clone current state
            _current = _current.Clone();
            //Clone global transitions
            for(int i =0; i < globalTransitions.Length; i++)
                globalTransitions[i] = globalTransitions[i].Clone();

            //Clear lists as no-longer needed
            temp_clonedStated = null;
            temp_clonedTransitions = null;
        }
        #endregion
    }
}