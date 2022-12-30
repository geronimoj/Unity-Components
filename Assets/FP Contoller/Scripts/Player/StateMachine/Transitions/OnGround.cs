using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checks if the player is on the ground
/// </summary>
[CreateAssetMenu(fileName = "OnGround", menuName = "Transitions/OnGround", order = 1)]
public class OnGround : OnGround_ElseAirborne
{
    /// <summary>
    /// Returns true if the player is on the ground
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns>Returns true if the player is moving downwards and a downwards SphereCast returns true or if isAirborn is false</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {
        if (Condition(ref ctrl))
            return true;
        return false;
    }
}
