using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;

/// <summary>
/// The state used for wall climbing
/// </summary>
[CreateAssetMenu(fileName = "WallClimb", menuName = "States/WallClimb", order = 3)]
public class WallClimbMove : State<PlayerController>
{   
    /// <summary>
    /// How long the player will wall climb for. This is calculated
    /// </summary>
    private float climbTimer = 0;
    /// <summary>
    /// A Timer for how long the player floats for after reaching the peak of the wall climb
    /// </summary>
    private float floatTimer = 0;
    /// <summary>
    /// Sets up the players movement direction and calculates how long the player will wallclimb for
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {
        //Set our movement direction to be directly up
        ctrl.Direction = Vector3.up;
        //Scale the climbing time by how much vertical speed we have left
        climbTimer = ((ctrl.maxDist - ctrl.minDist) * (ctrl.VertSpeed / ctrl.JumpForce)) + ctrl.minDist;
        climbTimer /= ctrl.climbSpeed;
        ctrl.VertSpeed = ctrl.climbSpeed;
        floatTimer = ctrl.floatTime;

        ToggleTransition(typeof(OnGround_ElseAirborne), true);
    }
    /// <summary>
    /// Moves the player up the wall. Checks the players vertical speed and if there is still a wall. If so, we exit wall climb move
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateUpdate(ref PlayerController ctrl)
    {   //Reduce our climb timer
        climbTimer -= Time.deltaTime;

        //If we are falling to fast or we are no-longer able to reach the wall, we cancel out of the wall climb and enter falling
        if (climbTimer <= 0 || !Physics.Raycast(ctrl.transform.position, ctrl.CheckDir, out RaycastHit hit, ctrl.CheckDirRange + 0.01f))
        {
            floatTimer -= Time.deltaTime;
            if (floatTimer <= 0)
            {
                //This should realistically only be called once because we re-enable the transitions
                Debug.Log("Exit wall climb");
                //Re-enable the ignored transitions
                ReEnableTransitions();
                //Make sure the player has no more vertical speed when they exit the floatyness
                ctrl.VertSpeed = 0;
                return;
            }
        }
        else
        {   //This is a bit un-necessary but we need it for if the player chooses to jump off of a given ledge
            ctrl.ExpectedDir = hit.normal;
            //Move the player up the wall
            ctrl.Move(ctrl.TotalVector * Time.deltaTime);
        }
    }

    protected override void StateEnd(ref PlayerController ctrl)
    {
        base.StateEnd(ref ctrl);

        if (ctrl.Direction != Vector3.up)
            //Rotate the camera to look away from the wall
            ctrl.ForceRotate((Mathf.Atan2(ctrl.ExpectedDir.x, ctrl.ExpectedDir.z) * Mathf.Rad2Deg) % 360);
    }
}
