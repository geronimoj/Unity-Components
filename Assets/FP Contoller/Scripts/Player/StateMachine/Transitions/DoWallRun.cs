using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.Transitions;

/// <summary>
/// A transition to represent if the player should enter a wall run
/// </summary>
[CreateAssetMenu(fileName = "DoWallRun", menuName = "Transitions/DoWallRun", order = 7)]
public class DoWallRun : Transition<PlayerController>
{
    /// <summary>
    /// Returns true if the player should perform a wall run
    /// </summary>
    /// <param name="ctrl">A reference to the player controller</param>
    /// <returns>Returns true if either of two raycasts outwards returns a hit and the player is looking parallel to the wall</returns>
    public override bool ShouldTransition(ref PlayerController ctrl)
    {   
        bool foundWall = false;
        Vector3 rayOffset = new Vector3(ctrl.Direction.z, 0, -ctrl.Direction.x) * ctrl.colInfo.Radius;
        Vector3 forward = ctrl.Direction;
        forward.y = 0;

        //Check the right side                                  //The direction of the raycast is forward with a bit of offset to point it outwards
        if (Physics.Raycast(ctrl.transform.position + rayOffset, (forward + rayOffset * 0.01f).normalized, out RaycastHit hit, ctrl.HozSpeed * Time.deltaTime + ctrl.colInfo.Radius)
            //Make sure the player is looking at the wall enough
            && Vector3.Dot(new Vector3(ctrl.transform.forward.x, 0, ctrl.transform.forward.z).normalized, hit.normal) > -0.87f)
        {   //Set our movement direction
            foundWall = true;
            ctrl.Direction = new Vector3(hit.normal.z, ctrl.Direction.y, -hit.normal.x);

            //Put the camera on a slight angle if we have access to its movement script
            if (ctrl.camFol != null)
                ctrl.camFol.rotationOffset.z = ctrl.cameraAngle;
        }
        //Check the left side                                       //The direction of the raycast is forward with a bit of offset to point it outwards
        else if (Physics.Raycast(ctrl.transform.position - rayOffset, (forward - rayOffset * 0.01f).normalized, out hit, ctrl.HozSpeed * Time.deltaTime + ctrl.colInfo.Radius)
            //Make sure the player is looking at the wall enough
            && Vector3.Dot(new Vector3(ctrl.transform.forward.x, 0, ctrl.transform.forward.z).normalized, hit.normal) > -0.87f)
        {   //Set our movement direction
            foundWall = true;
            ctrl.Direction = new Vector3(-hit.normal.z, ctrl.Direction.y, hit.normal.x);

            //Put the camera on a slight angle if we have access to its movement script
            if (ctrl.camFol != null)
                ctrl.camFol.rotationOffset.z = -ctrl.cameraAngle;
        }

        //If we found a wall, set are checkDir values
        if (foundWall)
        {
            Debug.Log("Wall Run");
            ctrl.CheckDir = -hit.normal;
            ctrl.CheckDirRange = Vector3.Distance(ctrl.transform.position, hit.point);
            return true;
        }

        return false;
    }
}
