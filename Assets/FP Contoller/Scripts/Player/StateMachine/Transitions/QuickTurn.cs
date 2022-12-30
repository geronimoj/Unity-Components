using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;

/// <summary>
/// Checks if the Quick Turn input was recently pressed
/// </summary>
[CreateAssetMenu(fileName = "QuickTurn", menuName = "Transitions/QuickTurn", order = 7)]
public class QuickTurn : Transition<PlayerController>
{
    /// <summary>
    /// Returns true when the Quick Turn input was pressed
    /// </summary>
    /// <param name="ctrl"> A reference to the player controller </param>
    /// <returns>Returns true when the Quick Turn input was pressed and if checkDir is not 0</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {   //Did the player press Q and had a checkDir and this is a new input
#if UNITY_STANDALONE_WIN
        if (InputManager.NewInput("Quick Turn") != 0 && ctrl.CheckDir != Vector3.zero)
#endif
#if UNITY_ANDROID
        if (InputManager.GetInput("Quick Turn") != 0 && ctrl.CheckDir != Vector3.zero)
#endif
            //Rotate the player
            ctrl.ForceRotate((Mathf.Atan2(-ctrl.CheckDir.x, -ctrl.CheckDir.z) * Mathf.Rad2Deg) % 360);
        //Return false because we don't ever want to transition using this transition
        return false;
    }
}
