using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;
using CustomController;

/// <summary>
/// The state for moving along the ground
/// </summary>
[CreateAssetMenu(fileName = "GroundMove", menuName = "States/GroundMove State", order = 1)]
public class GroundMoveState : State<PlayerController>
{
    private float timer = 0;
    /// <summary>
    /// Did we get an input this frame
    /// </summary>
    private bool gotInput = false;
    /// <summary>
    /// Did we get a new input
    /// </summary>
    private bool newInput = false;
    /// <summary>
    /// Should the player slow down
    /// </summary>
    private bool slowDown = false;
    /// <summary>
    /// A timer used when smoothing the rotation between expectedDir and currentDir
    /// </summary>
    private float slowDownTime = 0;
    /// <summary>
    /// The time since the last input
    /// </summary>
    private float inputTimer;
    /// <summary>
    /// A storage location for the movement vector
    /// </summary>
    private Vector3 moveVec = Vector3.zero;

    /// <summary>
    /// Set us up for moving horizontally
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {
        Debug.Log("OnGround");
        //Make sure our state is in the base state
        newInput = false;
        ctrl.direction.VertSpeed = 0;
    }
    /// <summary>
    /// Moves the player horizontally. Also up and down ramps
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateUpdate(ref PlayerController ctrl)
    {
        //Get the inputs
        float y = InputManager.GetInput("Horizontal");
        float x = InputManager.GetInput("Vertical");
        //Check if we got an input
        if (x != 0 || y != 0)
        {   //Store whether we got an input for use later
            gotInput = true;
            //Reset the timer because we don't want it going off
            inputTimer = ctrl.InputTime;
            //Get our movement direction
            Vector3 v = ctrl.GetAngle(x, y);
            //If we are moving in a new direction, reset the turn timer
            if (v != ctrl.ExpectedDir)
                timer = 0;
            ctrl.ExpectedDir = v;
        }
        else
        {   //Get our movement direction
            ctrl.ExpectedDir = Vector3.zero;
            //Store whether we got an input for use later
            gotInput = false;
            //Decrement the timer
            inputTimer -= Time.deltaTime;
            //If its less than 0, we consider that a new input is needed
            if (inputTimer <= 0)
                newInput = true;
        }
        /////////////ACCELERATION/////////////
        //If the direction the player wants to move in is in the opposite direction, we decellerate a bit
        slowDown = Vector3.Dot(ctrl.Direction, ctrl.ExpectedDir) < 0;
        //If we need to slowDown or we haven't gotten an input recently, decellerate, otherwise, accelerate
        if (slowDown || !gotInput && newInput)
        {   //Decellerate
            ctrl.Accelerate(true, true);
            slowDownTime = (-ctrl.HozSpeed) / -ctrl.Decelleration;
        }
        else
            //Accelerate
            ctrl.Accelerate(false, true);

        //Is the player stationary & only just made an input
        if (ctrl.HozSpeed <= 0 && newInput && gotInput)
        {
            //We got a new input so instantly start moving in the expected direction
            ctrl.Direction = ctrl.ExpectedDir;
            newInput = false;
        }
        else
        {   //Calculate our rotation time
            if (!slowDown)
                slowDownTime = ctrl.MinTurnTime;
            //Start rotating to the expected direction in a specific time frame
            ctrl.SmoothDirection(ctrl.ExpectedDir, slowDownTime, ref timer);
        }
        //Calculate the movement vector
        moveVec = ctrl.TotalVector * Time.deltaTime;
        //Check if we should move the player up or down the surface.
        if (CustomCollider.CastWithOffset(ctrl.colInfo, Vector3.down * (ctrl.StepHeight * 2), Vector3.up * ctrl.StepHeight + moveVec, out RaycastHit hit) && ctrl.colInfo.ValidSlope(hit.normal))
            moveVec.y = (hit.point + hit.normal * (ctrl.colInfo.Radius + ctrl.colInfo.CollisionOffset)).y - ctrl.colInfo.GetLowerPoint().y;

        //Move the character
        ctrl.Move(moveVec);
        ctrl.CheckDir = ctrl.Direction;
    }
}
