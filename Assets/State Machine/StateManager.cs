using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the states. Checks for transitions, swaps between states & calls update on the current state
/// </summary>
public class StateManager : MonoBehaviour
{
    /// <summary>
    /// A reference to the previous state in case we want to return to it
    /// </summary>
    public State previous;
    /// <summary>
    /// The current state
    /// </summary>
    public State current;
    /// <summary>
    /// The state we want to swap to. This is only public for debugging purposes
    /// </summary>
    public State target;
    /// <summary>
    /// These transitions will always be checked reguardless as to what state we are currently in
    /// </summary>
    public Transition[] globalTransitions;

    private bool start = false;
    /// <summary>
    /// Checks the transitions, swaps the current state if any return true.
    /// Then updates the current state if we have one
    /// </summary>
    public void DoState(PlayerController ctrl)
    {   //Represents the start function
        if (!start)
        {
            start = true;
            if (current != null)
                current.State_Start(ref ctrl);
        }

        //Don't do anything if we don't have a reference to the controller
        if (ctrl == null)
            return;
        //Make sure we have a state we can call
        if (current != null)
        {
            //Do we need to make a global transition                    //Do we need to make a transition out of the current state
            if (CheckTransitions(ref ctrl, ref globalTransitions, null) || CheckTransitions(ref ctrl, ref current.transitions, current.ignoreTransition))
                //Swap the states
                SwapStates(ref ctrl);
            //Call update on our current state
            current.State_Update(ref ctrl);
        }
    }
    /// <summary>
    /// Checks the transitions
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <param name="trans">The transitions that should be checked</param>
    /// <returns>Returns true on the first transition to return true</returns>
    bool CheckTransitions(ref PlayerController ctrl, ref Transition[] trans, bool[] ignore)
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
                target = trans[i].targetState;
                return true;
            }
        }
        //No transitions passed
        return false;
    }
    /// <summary>
    /// Swaps target state to current state
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    void SwapStates(ref PlayerController ctrl)
    {   //Make sure we have a valid controller
        if (ctrl == null || target == current || target == null)
            return;
        //Call end on our current state
        current.State_End(ref ctrl);
        //Swap our states around
        previous = current;
        current = target;
        target = null;
        //call state on our new state
        current.State_Start(ref ctrl);
    }
}
