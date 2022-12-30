using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The state for the basic step up action.
/// </summary>
[CreateAssetMenu(fileName = "StepUp", menuName = "States/StepUp", order = 1)]
public class StepUpMove : VaultBase
{
    protected override void SetMoveDistance(ref PlayerController ctrl, out float moveDist)
    {   //Set moveDist because we have to
        moveDist = ctrl.openSpaceRequired + ctrl.CheckDirRange;
    }
    protected override void CameraEffects(ref CameraFollower camFol, ref PlayerController ctrl)
    {   //Adjust the position of the camera over the course of the step up
        camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, 1 - (MoveDist / (ctrl.colInfo.Radius + ctrl.openSpaceRequired)));
    }

    protected override void StateEnd(ref PlayerController ctrl)
    {   //We have finished so reset the position of the collider
        //This should be seemless if we did the update function correctly
        ctrl.transform.position += ctrl.colInfo.PositionOffset;
        ctrl.colInfo.PositionOffset = Vector3.zero;
        ctrl.camFol.positionOffset = Vector3.zero;

        //Player go fast
        ctrl.direction.HozSpeed = ctrl.direction.MaxHozSpeed;
    }
}
