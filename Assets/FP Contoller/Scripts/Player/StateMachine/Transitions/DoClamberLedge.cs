using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;
/// <summary>
/// Checks if the player can climb onto a ledge they are currently grabbing
/// </summary>
[CreateAssetMenu(fileName = "DoClamber", menuName = "Transitions/DoClamber", order = 7)]
public class DoClamberLedge : Transition<PlayerController>
{
    /// <summary>
    /// Can the player climb up onto the ledge
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns>Returns true if there is enough solid ground and the player has the space bar pressed</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {   //Was the space bar pressed?
        if (InputManager.GetInput("Jump") != 0) 
            if (Physics.Raycast(ctrl.colInfo.GetHighestPoint() + ctrl.CheckDir * (ctrl.colInfo.Radius + ctrl.openSpaceRequired) + Vector3.up * 0.05f, Vector3.down, 0.1f))
            return true;
        return false;
    }
}
