using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;
/// <summary>
/// Checks if the player can grab a ledge
/// </summary>
[CreateAssetMenu(fileName = "AtLedge", menuName = "Transitions/AtLedge", order = 6)]
public class AtLedge : Transition<PlayerController>
{   
    /// <summary>
    /// Checks if the player is close enough to a ledge to grab it
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns>Returns false if the Crouch input is not 0</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {
        if (InputManager.GetInput("Crouch") != 0)
            return false;

        Vector3 checkDir;
        //If checkDir is not assigned a direction, use the players transform.forward
        if (ctrl.CheckDir == Vector3.zero)
        {
            checkDir = ctrl.transform.forward;
        }
        else
            checkDir = ctrl.CheckDir;
        //Make sure checkDir is horizontal and normalised
        checkDir.y = 0;
        checkDir.Normalize();
        //Store a vector to a position above the player
        Vector3 outPos = SetTopOfPlayer(ref ctrl);
        //Is there anything above the player?
        if (!Physics.Raycast(outPos, checkDir, ctrl.colInfo.TrueRadius + ctrl.atLedgeDistance)
            //Perform a raycast down to see if there is terrain below us
            && Physics.Raycast(outPos + checkDir * (ctrl.colInfo.TrueRadius + ctrl.atLedgeDistance), Vector3.down, out RaycastHit hit, ctrl.lowerGrabDist))
        {   //We have reached a ledge
            Debug.Log("At Ledge");
            //Get the direction we need to check on the horizontal plane
            Vector3 v = hit.point - ctrl.transform.position;
            v.y = 0;
            ctrl.CheckDir = v.normalized;
            //This is not mathematically perfect as it does not account for the players current position
            Vector3 move = hit.point + Vector3.down * ctrl.colInfo.UpperHeight;
            //Perform a raycast from our position just below the top of the ledge to get the distance to the ledge.
            Physics.Raycast(new Vector3(outPos.x, hit.point.y - 1e-5f, outPos.z), ctrl.CheckDir, out hit, ctrl.colInfo.TrueRadius + ctrl.atLedgeDistance);
            //We want to override the x & z position of the point we are going to move to but retain the height.
            //The previous raycast result gave us the top of the obstacle but the most recent one gave us the side.
            //Which we need to use to calculate the position offset to move the player to.
            move.x = hit.point.x;
            move.z = hit.point.z;
            //Calculate the positions horizontal position
            move -= ctrl.CheckDir *  ctrl.colInfo.TrueRadius;
            //Move the player to the destination
            ctrl.Move(move - ctrl.transform.position);

            return true;
        }
        return false;
    }
    /// <summary>
    /// Sets the location that defines the top of the player, this is the hieght the majority of the checks start from
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns></returns>
    protected virtual Vector3 SetTopOfPlayer(ref PlayerController ctrl)
    {
        return ctrl.colInfo.GetHighestPoint();
    }
}
