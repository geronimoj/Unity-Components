using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;

/// <summary>
/// The state for being in a wall run
/// </summary>
[CreateAssetMenu(fileName = "WallRunMove", menuName ="States/WallRun", order = 3)]
public class WallRun : State<PlayerController>
{   
    /// <summary>
    /// Forcefully rotates the players camera to look parallel to the wall. Makes sure we are setup for vertical movement
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {
        //Forcefully rotate the player to look directly forwards upon entering a wall run
        ctrl.ForceRotate((Mathf.Atan2(ctrl.Direction.x, ctrl.Direction.z) * Mathf.Rad2Deg) % 360);
    }
    /// <summary>
    /// Makes sure the wall is still there and moves the player along it.
    /// Calculates the jump angle if the player is looking behind themself or into the wall
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateUpdate(ref PlayerController ctrl)
    {   //Check that there is still a wall there. We do it from a bit below the players gameObject
        if (Physics.Raycast(ctrl.colInfo.GetLowerPoint(-ctrl.colInfo.LowerHeight / 2), ctrl.CheckDir, ctrl.CheckDirRange + 0.01f))
        {   //Reduce our vertical speed but with half the gravity
            ctrl.VertSpeed -= (ctrl.Gravity / 2) * Time.deltaTime;
            //Move them along currentDir. The horizontal plane is multiplied by hozSpeed but the y axis is multiplied by vertSpeed
            ctrl.Move(ctrl.TotalVector * Time.deltaTime);
        }
        else
        {   //There is no wall to wall run on so re-enable all the transitions
            //This should never be checked twice cause a transition should activate and take us out of this state
            Debug.Log("No more wall");
            ReEnableTransitions();
        }
        //Calculate the forward vector but only along the horizontal plane
        Vector3 forward = ctrl.transform.forward;
        forward.y = 0;
        forward.Normalize();
        ctrl.ExpectedDir = forward;

        //If we are looking into the wall, change the defualt jump angle, the direction of a jump is along ExpectedDir
        if (Vector3.Dot(forward, ctrl.CheckDir) >= 0)
            ctrl.ExpectedDir = ctrl.Direction - ctrl.CheckDir * ctrl.jumpOffPercent;    
        //We are looking away from the wall but are we looking behind ourself
        else if (Vector3.Dot(forward, ctrl.Direction) < 0)
            //If so, set our jump angle to be perpendicular to the wall
            ctrl.ExpectedDir = -ctrl.CheckDir;
    }

    protected override void StateEnd(ref PlayerController ctrl)
    {
        base.StateEnd(ref ctrl);
        //Undo the rotation on the camera
        if (ctrl.camFol != null)
            ctrl.camFol.rotationOffset.z = 0;
    }
}
