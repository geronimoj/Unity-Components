using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The base class for the High Vault and pull up actions.
/// Where the VaultBase is speed dependent, this is time dependent because of the added vertical component.
/// StateEnd still needs to be derived from to re-align the players hitbox and physical position.
/// Once all timers have finished, the state will re-enable the OnGroundElseAirborne transition
/// </summary>
public class HighVaultBase : VaultBase
{
    /// <summary>
    /// The time it takes for the player to be above the obstacle
    /// </summary>
    private float timeToPeak;
    /// <summary>
    /// The time it takes for the player to move over the obstacle.
    /// </summary>
    private float timeOver;
    /// <summary>
    /// The time it takes for the player to reach the ground after going over the obstacle.
    /// </summary>
    private float timeToGround;
    /// <summary>
    /// A timer for timeToPeak
    /// </summary>
    private float toPeakTimer;
    /// <summary>
    /// A timer for timeOver
    /// </summary>
    private float overTimer;
    /// <summary>
    /// A timer for timeToGround
    /// </summary>
    private float toGroundTimer;
    /// <summary>
    /// Returns how far through the ToPeak stage the HighVault is as a percentage. (0 - 1)
    /// Use for Camera stuff
    /// </summary>
    protected float ToPeakPercentComplete
    {
        get
        {
            return toPeakTimer / timeToPeak;
        }
    }
    /// <summary>
    /// Returns how far through the Over stage the HighVault is as a percentage. (0 - 1).
    /// Over is the time it takes to go over the obstacle, like a vault.
    /// Use for Camera stuff
    /// </summary>
    protected float OverPercentComplete
    {
        get
        {
            return overTimer / timeOver;
        }
    }
    /// <summary>
    /// Returns how far through the ToGround stage the HighVault is as a percentage. (0 - 1)
    /// Use for Camera stuff
    /// </summary>
    protected float ToGroundPercentComplete
    {
        get
        {
            return toGroundTimer / timeToGround;
        }
    }

    private float distanceToMoveFromObstacleEdge;

    protected override void StateStart(ref PlayerController ctrl)
    {
        base.StateStart(ref ctrl);
        //Update the timer values
        SetTimerValues(ref ctrl, out timeToPeak, out timeOver, out timeToGround);
        //Set all the timers to 0
        toPeakTimer = 0;
        overTimer = 0;
        toGroundTimer = 0;
    }
    /// <summary>
    /// The Start function that should be overrided for classes that derive from HighVaultBase. 
    /// Is necessary for the timers and distance to be assigned correct values.
    /// Will spit out an error if the timers haven't been set.
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <param name="moveDist">The distance the player needs to move to get over the obstacle. This does not include any vertical movement. Often the width of the obstacle + player radius.</param>
    /// <param name="toPeakTime">The time until the player is "above the obstacle"</param>
    /// <param name="overTime">The time it takes to move a distance of moveDist over the obstacle</param>
    /// <param name="toGroundTime">The time it takes to return back to the ground.</param>
    protected virtual void SetTimerValues(ref PlayerController ctrl, out float toPeakTime, out float overTime, out float toGroundTime)
    {
        toPeakTime = 0;
        overTime = 0;
        toGroundTime = 0;
        Debug.LogError("Timer values not set.");
        Debug.Break();
    }
    /// <summary>
    /// DO NOT OVERRIDE WITHOUT CALLING base.StateUpdate!
    /// Moves the player based on the information provided in timeToPeak, timeOver and timeToGround.
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateUpdate(ref PlayerController ctrl)
    {
        //Get the move vector
        Vector3 moveVec = ctrl.direction.TotalVector * Time.deltaTime;
        //Make sure we have reached the obstacle
        if (TotalMoveDist - MoveDist < ctrl.CheckDirRange - ctrl.colInfo.Radius)
        {   //If this movement this frame will take us past the edge of the obstacle, then we re-size moveVec to be the remaining distance to the obstacle.
            if (TotalMoveDist - MoveDist - moveVec.magnitude >= ctrl.CheckDirRange - ctrl.colInfo.Radius)
                moveVec = moveVec.normalized * (MoveDist - (TotalMoveDist - ctrl.CheckDirRange - ctrl.colInfo.Radius));
            //Move the player
            Move(moveVec, ref ctrl);
        }
        //We need to not move once we've reached the obstacle until timeToPeak has finished.
        else if (toPeakTimer < timeToPeak)
        {
            toPeakTimer += Time.deltaTime;
            //We are going to account for the overshoot in time in the timerOver if statement instead of here.
            //Otherwise, we may end up not moving far enough.
            distanceToMoveFromObstacleEdge = MoveDist;
        }
        //We don't put this as an else cause as we want to also enter timeOver stage if the toPeakTimer is exceeded.
        //This is separate to the overTimer if statement as we don't want to hit the toGround stage.
        //We could probably do this all with 1 timer but my brain isn't in the mood to figure it out.
        if (toPeakTimer >= timeToPeak)
        {
            //We need to move over the remaining distance in timeOver 
            if (overTimer < timeOver)
            {   
                overTimer += Time.deltaTime;
                //If we hit this block because toPeakTimer went overTime then increase overTimer by the excess of toPeakTimer.
                if (toPeakTimer >= timeToPeak)
                {
                    overTimer += toPeakTimer - timeToPeak;
                    toPeakTimer = timeToPeak;
                }
                //Remember to account for overShoot into the next stage
                if (overTimer > timeOver)
                {   //Update toGroundTimer to account for the overflow
                    toGroundTimer = overTimer - timeOver;
                    overTimer = timeOver;
                }
                //Calculate how far we need to move this frame based on the total distance we need to move
                moveVec = ctrl.direction.HozDirection * (distanceToMoveFromObstacleEdge / timeOver) * Time.deltaTime;
                //Move the player
                Move(moveVec, ref ctrl);
            }
            //Don't move until timeToGround is finished.
            else if (toGroundTimer < timeToGround)
            {
                toGroundTimer += Time.deltaTime;
                //Clamp the timer for the sake of the camera 
                if (toGroundTimer > timeToGround)
                    toGroundTimer = timeToGround;
            }
            //Otherwise we have finally finished so we can exit
            else
            {
                //Exit
                ToggleTransition(typeof(OnGround_ElseAirborne), false);
            }
        }

        //Perform any camera affects.
        CameraEffects(ref ctrl.camFol, ref ctrl);
    }
}
