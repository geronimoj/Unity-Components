using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;
using CustomController;

/// <summary>
/// The state for movement on a ledge
/// </summary>
[CreateAssetMenu(fileName = "LedgeMove", menuName = "States/OnLedge", order = 5)]
public class LedgeMove : State<PlayerController>
{   
    /// <summary>
    /// Sets the players horizontal and vertical speed to 0
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {
        base.StateStart(ref ctrl);
        ctrl.VertSpeed = 0;
        ctrl.HozSpeed = 0;
    }
    /// <summary>
    /// Moves the player along the ledge if they made an input.
    /// Enables and disables DoClamberLedge and DiDJump based on whether you are looking into or away from the ledge
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateUpdate(ref PlayerController ctrl)
    {   //Get the horizontal input and the forward vector on the player but without the y component
        float x = InputManager.GetInput("Horizontal");
        Vector3 forward = ctrl.transform.forward;
        forward.y = 0;
        forward.Normalize();
        //Set our jump vector to be the way we are looking
        ctrl.ExpectedDir = forward;

        //If the player pressed space, is looking into the wall and there is space above the wall for the player, pull the player on top of the wall
        if (Vector3.Dot(forward, ctrl.CheckDir) >= 0)
        {   //The player is looking into the wall
            ToggleTransition(typeof(DidJump), true);
            //Make sure there is space above the player if they want to pull up

            if (!CustomCollider.Cast(ctrl.colInfo, ctrl.CheckDir * (ctrl.colInfo.Radius * 2), Vector3.up * (ctrl.colInfo.Radius + ctrl.colInfo.Height + ctrl.colInfo.CollisionOffset)))
                ToggleTransition(typeof(DoClamberLedge), false);
            else
                ToggleTransition(typeof(DoClamberLedge), true);

            Vector3 right = new Vector3(ctrl.CheckDir.z, ctrl.CheckDir.y, -ctrl.CheckDir.x);
            //Do a raycast from the side of the player into the wall, continue if it returns true.
            if (x != 0 && Physics.Raycast(ctrl.colInfo.GetHighestPoint() + right * (x * ctrl.colInfo.Radius), ctrl.CheckDir, ctrl.CheckDirRange + 0.01f))
            {
                //Get the movement Vector
                Vector3 moveDir = right * x * ctrl.shimmySpeed * Time.deltaTime;
                //Do a raycast from above the player at the expected movement position, if it returns false, the ledge is still there so we are allowed to move
                if (Physics.Raycast(ctrl.colInfo.GetHighestPoint() + moveDir, ctrl.CheckDir, ctrl.CheckDirRange + 0.01f))
                    //Move the player
                    ctrl.Move(moveDir, true);
            }
        }
        else
        {   //The player is not looking into the wall so disable the pull up transition and enable jumping
            ToggleTransition(typeof(DidJump), false);
            ToggleTransition(typeof(DoClamberLedge), true);
        }
    }
}
