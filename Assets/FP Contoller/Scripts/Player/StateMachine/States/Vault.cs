using UnityEngine;
/// <summary>
/// The state for the Vault
/// </summary>
[CreateAssetMenu(fileName = "Vault", menuName = "States/Vault",  order = 1)]
public class Vault : VaultBase
{
    /// <summary>
    /// The time when entering a vault, before the vault is determined.
    /// </summary>
    public float vaultEntryTime = 0.1f;
    private float entryTimer = 0;
    /// <summary>
    /// The angle the camera swaps to when entering a vault.
    /// </summary>
    public float vaultCameraAngle = -20;
    /// <summary>
    /// The lowerHeight of the player when they entered this state.
    /// </summary>
    private float originalLowerHeight;
    /// <summary>
    /// The distance from the closest point from the obstacle, the player must move, assuming they are right next to the obstacle.
    /// </summary>
    private float vaultDistFromObstacle = 0;

    private VaultType type;

    private bool actionInputPressed = false;
    private Vector3 camOriginalPositionOffset = Vector3.zero;

    protected override void StateStart(ref PlayerController ctrl)
    {
        base.StateStart(ref ctrl);
        //Set the hitboxes infromation and store its original size
        originalLowerHeight = ctrl.colInfo.LowerHeight;
        ctrl.colInfo.LowerHeight = ctrl.PlayerVaultHeight - ctrl.colInfo.UpperHeight;
        //Set the distance from the edge of the obstacle. Need it for math later
        vaultDistFromObstacle = ctrl.colInfo.Radius + ctrl.VaultDistance;

        type = VaultType.SpeedVault;

        entryTimer = 0;
    }

    protected override void SetMoveDistance(ref PlayerController ctrl, out float moveDist)
    {   
        //Store the distance we need to move
        moveDist = ctrl.VaultDistance + ctrl.CheckDirRange;
    }

    protected override void CameraEffects(ref CameraFollower camFol, ref PlayerController ctrl)
    {
        Debug.Log("Vault Type: " + type);
        //Set the cameras position
        switch (type)
        {
            case VaultType.SpeedVault:
                if (MoveDist > (vaultDistFromObstacle / 3) * 2)
                {
                    camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, 3 * (1 - (MoveDist / vaultDistFromObstacle)));
                    camFol.rotationOffset.z = Mathf.Lerp(0, vaultCameraAngle, 3 * (1 - (MoveDist / vaultDistFromObstacle)));
                }
                else if (MoveDist < (vaultDistFromObstacle / 3))
                {
                    camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, 3 * ((MoveDist / vaultDistFromObstacle)));
                    camFol.rotationOffset.z = Mathf.Lerp(0, vaultCameraAngle, 3 * ((MoveDist / vaultDistFromObstacle)));
                }
                break;
            case VaultType.MonkeyVault:
                //We put it inside a quick bool clause to save a bit of time looping through all the inputs
                if (actionInputPressed == false)
                    actionInputPressed = InputManager.NewInput("Jump") != 0;
                //We want to raise up during the first third and come back down in the final third of the vault
                if (MoveDist > (vaultDistFromObstacle / 3) * 2)
                {
                    camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, 3 * (1 - (MoveDist / vaultDistFromObstacle)));
                }
                else if (MoveDist < (vaultDistFromObstacle / 3))
                {
                    camFol.positionOffset = Vector3.Lerp(Vector3.zero, ctrl.colInfo.PositionOffset, 3 * ((MoveDist / vaultDistFromObstacle)));
                }
                else
                {   //Since we must be in the middle of the vault, enable the jump transition
                    ToggleTransition(typeof(DidJump), true);
                    //If the player previously pressed the jump key while in this state, force the next jump since the input will not be recorded as a new input for the DidJump transition
                    if (actionInputPressed)
                        ctrl.ForceJump = true;
                }
                break;
            case VaultType.SwitchVault:

                if (MoveDist > (vaultDistFromObstacle / 3) * 2)
                {
                    camFol.positionOffset = Vector3.Lerp(camOriginalPositionOffset, ctrl.colInfo.PositionOffset, 3 * (1 - (MoveDist / vaultDistFromObstacle)));
                    camFol.rotationOffset.z = Mathf.Lerp(0, vaultCameraAngle, 3 * (1 - (MoveDist / vaultDistFromObstacle)));
                }
                else if (MoveDist < (vaultDistFromObstacle / 3))
                {
                    ctrl.ForceRotate((Mathf.Atan2(-ctrl.direction.HozDirection.x, -ctrl.direction.HozDirection.z) * Mathf.Rad2Deg) % 360);
                    camFol.positionOffset = Vector3.Lerp(camOriginalPositionOffset, ctrl.colInfo.PositionOffset, 3 * ((MoveDist / vaultDistFromObstacle)));
                    camFol.rotationOffset.z = Mathf.Lerp(0, vaultCameraAngle, 3 * ((MoveDist / vaultDistFromObstacle)));
                }
                break;
        }
        //Differences between Monkey Vault, Speed Vault, 180 Vault.
        //Monkey Vault enables the jump action at specific windows of opportunity.
        //Speed Vault does nothing.
        //180 Vault ends with the player grabbing the ledge.
    }

    protected override void StateUpdate(ref PlayerController ctrl)
    {
        base.StateUpdate(ref ctrl);
        if (type == VaultType.SpeedVault && entryTimer < vaultEntryTime)
        {
            entryTimer += Time.deltaTime;
            if (InputManager.GetInput("Crouch") != 0)
            {
                actionInputPressed = false;
                type = VaultType.MonkeyVault;
            }
            else if (InputManager.GetInput("Grab") != 0
                //Make sure we can perform the switch vault or 180 vault.
                && !Physics.SphereCast(ctrl.colInfo.GetOriginPosition() + ctrl.direction.HozDirection * MoveDist,
                                        ctrl.colInfo.Radius + ctrl.colInfo.CollisionOffset,
                                        Vector3.down,
                                        out _,
                                        //We assume that the bottom of the players collider is basically at the same height as the obstacles
                                        ctrl.colInfo.Height + ctrl.colInfo.LowerHeight))
            {
                //There is enough space to perform the 180 vault.
                type = VaultType.SwitchVault;
                //Since we are doing a 180 vault, we need to know the distance to the other side of the obstacle for the ledge move when we swap into it at the end
                if (!Physics.Raycast(ctrl.transform.position + ctrl.direction.HozDirection * MoveDist + Vector3.down * (ctrl.colInfo.Height + originalLowerHeight)
                    , -ctrl.direction.HozDirection,
                    out RaycastHit h,
                    MoveDist))
                {
                    //Let somebody know that the switch vault broke a bit
                    Debug.LogError("Obstacle not found. Switch Vault/180Vault cannot assign propper values for the AtLedge transition check later.");
                }
                else
                {
                    ctrl.CheckDir = -h.normal;
                    ctrl.CheckDirRange = h.distance;
                }
                //Now all we need to do is make sure the players origin is positioned correctly before we check AtLedge.
                //The correct position is where the top of their collider is only just above the ledge.
                //We can just use height since the players height has already changed. Since we are only changing the players lower height, it makes it easier.
                //We take the current position offset. We assume that the bottom of the players collider is basically just above the obstacle so we can use that to move the origin just below the ledge, like I said 2 lines up
                Vector3 offsetToApply = ctrl.colInfo.PositionOffset + (Vector3.down * ctrl.colInfo.Height);
                //Now the offsetToApply is the direction and how much by, we need to move the player to be below the top of the obstacle
                ctrl.transform.position += offsetToApply;
                //We need to apply the inverse to the players collider offset else it won't be the right height for the vault and might cause collision problems
                ctrl.colInfo.PositionOffset -= offsetToApply;
                ctrl.camFol.positionOffset = -offsetToApply;
                camOriginalPositionOffset = ctrl.camFol.positionOffset;

                //NOTE: IF ATLEDGE TRANSITION IS EVER CHANGED TO USE ctrl.colInfo.GetOriginPosition() INSTEAD OF ctrl.transform.position
                //THIS WILL HAVE TO BE UPDATED.
            }
        }

        //Check if we have finished the vault. We have to do this here because otherwise the this check will never be true since the player moves after the affects of CameraEffects
        if (type == VaultType.SwitchVault && TransitionEnabled(typeof(SwitchVaultToLedgeGrab)) && !Physics.SphereCast(ctrl.colInfo.GetOriginPosition(), ctrl.colInfo.TrueRadius, Vector3.down, out _,ctrl.colInfo.Height + originalLowerHeight))
        {
            //This enables the AtLedge transition which should be checked just before OnGroundElseAirborne
            ToggleTransition(typeof(SwitchVaultToLedgeGrab), false);
#if UNITY_EDITOR
            Debug.DrawRay(ctrl.colInfo.GetLowestPoint(), Vector3.up, Color.green,10f);
#endif
        }
    }

    protected override void StateEnd(ref PlayerController ctrl)
    {
        //Undo our changes to the player collider
        ctrl.colInfo.LowerHeight = originalLowerHeight;

        //If we exited via a jump, we need to move the player back up like a Step Up instead of moving the collider down like a Vault
        if (ctrl.VertSpeed != 0)
        {
            ctrl.transform.position += ctrl.colInfo.PositionOffset;
            ctrl.colInfo.PositionOffset = Vector3.zero;
        }
        else
            ctrl.colInfo.PositionOffset = Vector3.zero;
        //Set the players speed to the max
        ctrl.direction.HozSpeed = ctrl.direction.MaxHozSpeed;
        //Reset the camera 
        ctrl.camFol.rotationOffset = Vector3.zero;
        ctrl.camFol.positionOffset = Vector3.zero;
    }
}

enum VaultType
{
    SpeedVault = 0,
    MonkeyVault = 1,
    //180 Vault because we can't put 180 at the beginning
    SwitchVault = 2
}