using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A test class for testing the HighVaultBase. Its programmed as a regular vault however.
/// </summary>
[CreateAssetMenu(fileName = "HighVaultTest", menuName = "States/Test States/HighVault", order = 0)]
public class HighVaultTest : HighVaultBase
{
    private float originalLowerHeight;
    private bool spinVault = false;

    protected override void StateStart(ref PlayerController ctrl)
    {
        base.StateStart(ref ctrl);
        spinVault = false;
        //Set the hitboxes infromation and store its original size
        originalLowerHeight = ctrl.colInfo.LowerHeight;
        ctrl.colInfo.LowerHeight = ctrl.PlayerVaultHeight - ctrl.colInfo.UpperHeight;
    }
    protected override void SetTimerValues(ref PlayerController ctrl, out float toPeakTime, out float overTime, out float toGroundTime)
    {
        toPeakTime = 0.5f;
        overTime = 0.5f;
        toGroundTime = 0.5f;
    }

    protected override void CameraEffects(ref CameraFollower camFol, ref PlayerController ctrl)
    {
        if (ToPeakPercentComplete != 1)
        {
            camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, ToPeakPercentComplete);
            //Check if the player wants to do a spin vault
            spinVault = InputManager.GetInput("Grab") != 0;
            if (spinVault && Physics.Raycast(ctrl.transform.position + ctrl.direction.HozDirection * MoveDist, -ctrl.direction.HozDirection, out RaycastHit h, MoveDist))
            {
                ctrl.CheckDir = -h.normal;
                ctrl.CheckDirRange = h.distance;
            }
      }
      else
      {
          camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, 1 - ToGroundPercentComplete);
            for (int i = 0; i < transitions.Length && spinVault; i++)
                ToggleTransition(typeof(SwitchVaultToLedgeGrab), false);
        }

        if (spinVault && OverPercentComplete > 0.9f)
            ctrl.ForceRotate((Mathf.Atan2(-ctrl.direction.HozDirection.x, -ctrl.direction.HozDirection.z) * Mathf.Rad2Deg) % 360);

    }

    protected override void SetMoveDistance(ref PlayerController ctrl, out float moveDist)
    {
        //Store the distance we need to move
        moveDist = ctrl.VaultDistance + ctrl.CheckDirRange;
    }


    protected override void StateEnd(ref PlayerController ctrl)
    {   //Undo our changes to the player collider
        ctrl.colInfo.LowerHeight = originalLowerHeight;
        ctrl.colInfo.PositionOffset = Vector3.zero;
        ctrl.camFol.positionOffset = Vector3.zero;
        //Set the players speed to the max
        ctrl.direction.HozSpeed = 0;
    }
}
