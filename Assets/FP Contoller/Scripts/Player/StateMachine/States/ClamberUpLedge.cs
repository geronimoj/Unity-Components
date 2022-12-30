
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the player up and over an already specified ledge
/// </summary>
[CreateAssetMenu(fileName = "ClamberLedge", menuName = "States/ClamberLedge", order = 6)]
public class ClamberUpLedge : HighVaultBase
{
    /// <summary>
    /// The time it takes to climb up the ledge, so you are level with it
    /// </summary>
    public float timeToClimbUp = 0.5f;
    /// <summary>
    /// The time it takes to climb onto the ledge after climbing up it
    /// </summary>
    public float timeToClimbOnTo = 0.5f;
    /// <summary>
    /// Sets the movement direction, CheckDirRange to 0 and the position offset of the collider
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {
        base.StateStart(ref ctrl);
        //Set our movement vector to be into the wall so we move. 
        ctrl.direction.Direction = ctrl.CheckDir;
        //Set CheckDirRange to be 0. This is because it is used to determine if we are close enough to the wall, which we already know we are.
        ctrl.CheckDirRange = 0;
        //Since we will be coming into this state likely from a ledgeMove, we need to set the collider offset here since this is techniqually a HighVault
        ctrl.colInfo.PositionOffset = Vector3.up * (ctrl.colInfo.Height + ctrl.colInfo.CollisionOffset);
    }
    /// <summary>
    /// Sets moveDist to be the PlayerControllers openSpaceRequired variable
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <param name="moveDist">The distance this "highVault" will move us</param>
    protected override void SetMoveDistance(ref PlayerController ctrl, out float moveDist)
    {
        moveDist = ctrl.openSpaceRequired;
    }
    /// <summary>
    /// Sets the timer values. ToGroundTime is always set to 0. ToPeak is set to timeToClimbUp and overTime is set to timeToClimbOnTo
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <param name="toPeakTime">The time to reach the peak of the climb</param>
    /// <param name="overTime">The time to climb onto the ledge</param>
    /// <param name="toGroundTime">Defaults to 0 anyways but would otherwise act as a pause after climbing up and over a ledge</param>
    protected override void SetTimerValues(ref PlayerController ctrl, out float toPeakTime, out float overTime, out float toGroundTime)
    {
        toPeakTime = timeToClimbUp;
        overTime = timeToClimbOnTo;
        toGroundTime = 0;
    }
    /// <summary>
    /// Moves the camera up during the ToPeak phase
    /// </summary>
    /// <param name="camFol">A reference to the camera controller</param>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void CameraEffects(ref CameraFollower camFol, ref PlayerController ctrl)
    {
        camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, ToPeakPercentComplete);
    }
    /// <summary>
    /// Resets the players collider when they finish the climb up the ledge
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateEnd(ref PlayerController ctrl)
    {   //We have finished so reset the position of the collider
        //This should be seemless if we did the update function correctly
        ctrl.transform.position += ctrl.colInfo.PositionOffset;
        ctrl.colInfo.PositionOffset = Vector3.zero;
        ctrl.camFol.positionOffset = Vector3.zero;

        //Player should have to start accelerating
        ctrl.direction.HozSpeed = 0;
    }
}
