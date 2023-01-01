using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;
/// <summary>
/// The base for the players regular vault.
/// </summary>
[CreateAssetMenu(fileName = "VaultBase", menuName = "States/VaultBase", order = 0)]
public class VaultBase : State<PlayerController>
{
    /// <summary>
    /// The speed at which the player moves during this state. This is still limited by max hoz speed
    /// </summary>
    public float moveSpeedDuringState = 5.5f;
    /// <summary>
    /// The remaining distance to move until we reach the end of the vault
    /// </summary>
    private float moveDist = 0;
    /// <summary>
    /// The total distance we must move.
    /// </summary>
    private float totalMoveDist = 0;
    /// <summary>
    /// Returns moveDist. Its readonly so derived classes can't adjust moveDist
    /// MoveDist: The remaining distance the player has to move before exiting this state
    /// </summary>
    protected float MoveDist
    {
        get
        {
            return moveDist;
        }
    }
    /// <summary>
    /// Returns totalMoveDist. Its readonly so derived classes can't adjust totalMoveDist
    /// TotalMoveDist: The total distance from entering this state the player has to move to exit the state
    /// </summary>
    protected float TotalMoveDist
    {
        get
        {
            return totalMoveDist;
        }
    }

    /// <summary>
    /// Called when the state is entered. YOU NEED TO SET MoveDist
    /// Other Things performed before this function is called
    /// The OnGround_ElseAirborne transition is set to ignored.
    /// VertSpeed is set to 0.
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {   
        //Make sure we don't have any vertical speed
        ctrl.VertSpeed = 0;
        //Then set it to be ignored
        ToggleTransition(typeof(OnGround_ElseAirborne), true);
        //Set the players speed to the vault speed
        ctrl.direction.HozSpeed = moveSpeedDuringState;
        //Call any derived functions stuffs
        SetMoveDistance(ref ctrl, out moveDist);
        totalMoveDist = moveDist;
    }
    /// <summary>
    /// Sets the distance the player needs to move. Spits out an error if moveDist has not been set.
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <param name="moveDist">The distance the player will move throughout the vault</param>
    protected virtual void SetMoveDistance(ref PlayerController ctrl, out float moveDist) 
    { 
        moveDist = 0;
        Debug.LogError("Move Distance not set.");
        Debug.Break();
    }

    protected override void StateUpdate(ref PlayerController ctrl)
    {   //Make sure we have reached the obstacle
        if (totalMoveDist - moveDist >= ctrl.CheckDirRange - ctrl.colInfo.Radius)
            //Perform any camera affects.
            CameraEffects(ref ctrl.camFol, ref ctrl);
        //Get the move vector
        Vector3 moveVec = ctrl.direction.TotalVector * Time.deltaTime;
        //Move the player
        Move(moveVec, ref ctrl);
        //Check if we have reached the end of the vault
        if (moveDist <= 0)
        {
            //Exit
            ToggleTransition(typeof(OnGround_ElseAirborne), false);
        }
    }
    /// <summary>
    /// Called every frame when the camera should be updated. The camera will be updated when the player is close enough to the low ledge.
    /// The call rate of this function is dependent on checkDirRange. Do not change checkDirRange.
    /// This can also be used somewhat as an update function for the state
    /// </summary>
    /// <param name="camFol">A reference to the camera follower</param>
    /// <param name="ctrl">A reference to the player controller</param>
    protected virtual void CameraEffects(ref CameraFollower camFol, ref PlayerController ctrl) { }
    /// <summary>
    /// Moves the player character but limited by the vault distance
    /// </summary>
    /// <param name="moveVec">The vector to move along</param>
    /// <param name="c">A reference to the player controller</param>
    protected void Move(Vector3 moveVec, ref PlayerController c)
    {
        //If we pass the final destination, resize moveVec so we don't
        if (moveDist - moveVec.magnitude < 0)
            moveVec = moveVec.normalized * moveDist;
        //Reduce the distance left to move
        moveDist -= moveVec.magnitude;
        //Move the player
        c.Move(moveVec);
    }
}