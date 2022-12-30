using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Checks if the player is airborn
/// </summary>
[CreateAssetMenu(fileName = "AmFalling", menuName = "Transitions/AmFalling", order = 2)]
public class AmFalling : OnGround_ElseAirborne
{
    /// <summary>
    /// Checks if there is terrain below the player, if not, it returns true
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns>Returns true if isAirborn is true</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {
        if (Condition(ref ctrl))
            return false;

        return true;
    }
}
