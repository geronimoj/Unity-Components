using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.States;

/// <summary>
/// Move the player assuming they are airborn
/// </summary>
[CreateAssetMenu (fileName = "AirbornMove", menuName = "States/AirbornMove", order = 2)]
public class AirbornMove : State<PlayerController>
{
    /// <summary>
    /// Setup the player for vertical falling
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateStart(ref PlayerController ctrl)
    {
        Debug.Log("Airborne");
        //We assign expectedDir first so that we can use it for checks with DoWallClimb
        ctrl.ExpectedDir = ctrl.Direction;
        ctrl.CheckDir = ctrl.Direction;
    }
    /// <summary>
    /// Move the player forward and apply gravity
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    protected override void StateUpdate(ref PlayerController ctrl)
    {   //Reduce the players vertical speed by their gravity
        ctrl.VertSpeed -= ctrl.Gravity * Time.deltaTime;
        //Move them along currentDir. The horizontal plane is multiplied by hozSpeed but the y axis is multiplied by vertSpeed
        ctrl.Move(ctrl.TotalVector * Time.deltaTime);
    }
}
