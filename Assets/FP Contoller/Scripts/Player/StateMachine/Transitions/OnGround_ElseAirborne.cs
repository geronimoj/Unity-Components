using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;

[CreateAssetMenu(fileName = "OnGroundElseAirborne", menuName = "Transitions/OnGroundElseAirborne", order = 0)]
public class OnGround_ElseAirborne : If_ElseTransition<PlayerController>
{
    protected override bool Condition(ref PlayerController ctrl)
    {
        //If our raycast hits something, that means there is something to land on so land on it && Make sure we are moving down
        if (ctrl.VertSpeed <= 0)
        {
            /*                                                                                                        //We raycast a tiny bit further because some terrains seems to have a floating point error
            if (ColliderInfo.Cast(ctrl.colInfo, Vector3.down * (Mathf.Abs(ctrl.VertSpeed) * Time.deltaTime + ctrl.colInfo.CollisionOffset), out RaycastHit hit))
            {   //Set the players y position to be equal to their lowerHeight above the point
                ctrl.transform.position = new Vector3(ctrl.transform.position.x, hit.point.y + (ctrl.colInfo.LowerHeight + ctrl.colInfo.CollisionOffset), ctrl.transform.position.z);
                ctrl.PreviousGround = hit.point;
                return true;
            }
            */
            if (ctrl.colInfo.OnGround)
                return true;

            Vector3 dif = ctrl.PreviousGround - ctrl.colInfo.GetLowestPoint();
            if (Conditions.InTolerance(dif.y, 0, ctrl.colInfo.CollisionOffset) && dif.magnitude < ctrl.colInfo.Radius)
                return true;
        }
        return false;
    }
}
