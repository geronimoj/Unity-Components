using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;
using CustomController;
/// <summary>
/// Checks if the player can vault or step up ledges. These checks are combined because they require the same height
/// </summary>
[CreateAssetMenu(fileName = "VaultOrStep", menuName = "Transitions/VaultOrStep", order = 5)]
public class Vault_Or_StepUp : If_ElseIfTransition<PlayerController>
{
    public float checkDist = 0.2f;
    /// <summary>
    /// Is set to true if the player pressed the space bar
    /// </summary>
    private bool didJump = false;
    /// <summary>
    /// Set to true if there is a low ledge and there is space above it.
    /// </summary>
    private bool hitLedge = false;
    /// <summary>
    /// The hit info for the top of the obstacle. Necessary for later calculations.
    /// </summary>
    private RaycastHit topOfObstacle;
    /// <summary>
    /// The distance from the player to the obstacle
    /// </summary>
    private float distanceToObstacle = 0;

    /// <summary>
    /// Checks if the player pressed the jump key and if they have reached a ledge.
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    protected override bool UniversalChecks(ref PlayerController c)
    {
        return CheckForVaultableorLedge(ref c, c.StepHeight, c.LowLedgeHeight);
    }
    /// <summary>
    /// Checks if the earlier checks passed & also if there is space on the other side for a landing
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <returns>Returns true if the player can vault</returns>
    protected override bool IfCondition(ref PlayerController c)
    {
        if (CheckVaultCondition(ref c, c.VaultDistance, c.PlayerVaultHeight))
        {
            Debug.Log("Can Vault");
            return true;
        }
        return false;
    }
    /// <summary>
    /// Checks if the earlier checks passed, meaning we are doing a step up
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <returns>Returns true if the player can step up onto the ledge</returns>
    protected override bool IfElseCondition(ref PlayerController c)
    {
        //We do it from the bottom of the collider
        Vector3 castOffset = topOfObstacle.point - c.colInfo.GetLowerPoint();
        //Move it up a bit so its not directly on top of the surface
        castOffset += Vector3.up * (c.colInfo.Radius + c.colInfo.CollisionOffset);
        //Make sure we aren't moving the origin of the cast forward
        castOffset.x = 0;
        castOffset.z = 0;

        //Perform a capsual cast assuming the player's normal hitbox.
        if (!CustomCollider.CastWithOffset(c.colInfo, c.direction.HozDirection * (c.openSpaceRequired + distanceToObstacle), castOffset))
        {
            //Since we are swapping into the Step Up, we need to set the position offset of the collider here since we won't know about it later.
            c.colInfo.PositionOffset = castOffset;
            //Assign the distance to the object we want to step up
            c.CheckDirRange = distanceToObstacle;
            Debug.Log("Can Step Up");
            return true;
        }
        //If it hits nothing, return a success.
        //Will do later 
        //Otherwise, perform a capsual cast assuming the player's sliding hitbox but with a shorter distance, being just enough space for the player to fit when crouched.
        //If the cast hits nothing, return a success, otherwise fail.
        return false;
    }
    /// <summary>
    /// The only function called in UniversalChecks. This is a function because the derived class, HighVaultorLedgeRoll, needs to change
    /// the lowerRange and upperRange
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <param name="lowerRange">The height, an offset from the bottom of the player pointing upwards, that determines the lower range for the area to check</param>
    /// <param name="upperRange">The height, an offset from the bottom of the player pointing upwards, that determines the upper range for the area of the check </param>
    /// <returns>Returns false if the obstacle is not vaultable or a ledge, if it hits nothing, or if the jump key is not pressed</returns>
    protected bool CheckForVaultableorLedge(ref PlayerController c, float lowerRange, float upperRange)
    {
        didJump = InputManager.NewInput("Jump") != 0;
        //If we didn't get a jump input, there is no point in doing these checks.
        if (!didJump)
            return false;
        Debug.DrawRay(c.colInfo.GetLowerPoint(), Vector3.up, Color.blue, 20f);
        float checkRange = upperRange - lowerRange;
        //Perform a raycast between lowerStepHeight & maxStepHeight to check if we collide with anything.
        hitLedge = Physics.BoxCast(c.colInfo.GetLowestPoint() + Vector3.up * (lowerRange + (checkRange / 2)),
            //Set the dimensions of the box
            new Vector3(c.colInfo.Radius / 2, checkRange / 2, c.colInfo.Radius),
            //Raycast direction
            c.direction.HozDirection,
            //We re-use topOfObstacle to save on a bit of memory since it will be overwritten if we succeed in the check
            out topOfObstacle,
            //Box orientation
            Quaternion.LookRotation(c.direction.HozDirection, Vector3.up),
            //The distance of the check
            checkDist);
        //If we don't hit anything, theres no point in continuing these checks.
        if (!hitLedge)
            return false;
#if UNITY_EDITOR
        //Draw a red line between the top and bottom range at the obstacle we hit. This is extremely inefficient because its only debug code
        Debug.DrawLine(topOfObstacle.point - topOfObstacle.normal * 1e-5f + Vector3.up * (upperRange - (topOfObstacle.point.y - c.colInfo.GetLowestPoint().y)), topOfObstacle.point - topOfObstacle.normal * 1e-5f + Vector3.up * (upperRange - (topOfObstacle.point.y - c.colInfo.GetLowestPoint().y)) + Vector3.down * checkRange, Color.red, 50f);
#endif
        distanceToObstacle = Vector3.Dot(c.direction.HozDirection.normalized, topOfObstacle.point - c.colInfo.GetLowerPoint());
        //Perform another raycast to find the top of the surface we hit earlier.
        hitLedge = Physics.Raycast(
            //Move the ray in a bit from the hit point and up be at equal height of lowLedgeHeight. Yea its a bit of a math block
            topOfObstacle.point - topOfObstacle.normal * 1e-5f + Vector3.up * (upperRange - (topOfObstacle.point.y - c.colInfo.GetLowestPoint().y)),
            Vector3.down,
            //Store the height of the top of the surface so we don't have to do it later.
            out topOfObstacle,
            checkRange);

        //If we again didn't hit anything. Then this must be a plane, very thin object or the height is out of range.
        //Either way, we don't want to continue. So return false.
        if (!hitLedge)
            return false;
        //All the checks for a lowLedge passed so return a success and get onto checking if we can vault or step up this low ledge.
        return true;
    }
    /// <summary>
    /// The function that checks the conditions for the vault. This has been separated because the derived class HighVaultorLedgeRoll needs to change the distance and playerHeight
    /// </summary>
    /// <param name="c">A reference to the player controller</param>
    /// <param name="vDistance">The distance the player will move during the vault</param>
    /// <param name="playerHeightDuringVault">The height of the player during the vault. Necessary for ensuring the checks make sense</param>
    /// <returns>Returns false if you cannot vault</returns>
    protected bool CheckVaultCondition(ref PlayerController c, float vDistance, float playerHeightDuringVault)
    {
        //Calculate the offset we need to apply to the collider
        //We do it from the bottom of the collider assuming its vault height
        Vector3 castOffset = topOfObstacle.point - c.colInfo.GetLowerPoint(playerHeightDuringVault - c.colInfo.Height);
        //Move it up a bit so its not directly on top of the surface
        castOffset += Vector3.up * (c.colInfo.Radius + c.colInfo.CollisionOffset + 1e-5f);
        //Make sure we aren't moving the raycast forward, only up to be level with the obstacle
        castOffset.x = 0;
        castOffset.z = 0;
        //Perform a capsual cast with the vault hitbox information
        //Since we are using a different height, we need to do a capsual cast :(
        if (Physics.CapsuleCast(
            //Get the lowerPoint offset by the difference in lower height. Add castOffset so its the correct height
            c.colInfo.GetLowerPoint(playerHeightDuringVault - c.colInfo.Height) + castOffset,
            c.colInfo.GetUpperPoint() + castOffset,
            c.colInfo.Radius + c.colInfo.CollisionOffset,
            //The direction we are looking
            c.direction.HozDirection,
            //Account for the distance of the vault and the distance we are from the collision offset
            vDistance + distanceToObstacle))
            //If we hit somethign return false
            return false;
        //If it doesn't hit anything, perform a capsual cast straight down to check and make sure there is no ground
        if (Physics.SphereCast(
            c.colInfo.GetLowerPoint(playerHeightDuringVault - c.colInfo.Height) + castOffset + c.direction.HozDirection * (vDistance + distanceToObstacle),
            c.colInfo.Radius + c.colInfo.CollisionOffset,
            Vector3.down,
            out _,
            castOffset.y))
            return false;
        //Since we are swapping into the Vault, we need to set the position offset of the collider here since we won't know about it later.
        c.colInfo.PositionOffset = castOffset;
        //Assign the distance to the object we want to vault over
        c.CheckDirRange = distanceToObstacle;
        //If that also returns false, return a success.
        return true;
    }
}