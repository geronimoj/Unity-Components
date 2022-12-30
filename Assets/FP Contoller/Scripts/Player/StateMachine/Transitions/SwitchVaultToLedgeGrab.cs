using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Checks to see when, if possible, which it should, if a switch vault can transition into a ledge grab
/// </summary>
[CreateAssetMenu(fileName = "SwitchVaultLedgeGrab", menuName = "Transitions/SwitchVaultToLedgeGrab", order = 6)]
public class SwitchVaultToLedgeGrab : AtLedge
{
    protected override Vector3 SetTopOfPlayer(ref PlayerController ctrl)
    {
        return ctrl.colInfo.GetLowestPoint();
    }
}
