using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;

/// <summary>
/// Checks if the Crouch input was recently pressed
/// </summary>
[CreateAssetMenu(fileName = "PressedShift", menuName = "Transitions/PressedShift", order = 9)]
public class PressedShift : Transition<PlayerController>
{
    /// <summary>
    /// Returns true when the Crouch input was pressed
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns>Returns true when a new Crouch input was made</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {
        if (InputManager.NewInput("Crouch") != 0)
            return true;
        return false;
    }
}
