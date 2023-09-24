using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets the position and rotation of this gameObject to be identical to the target
/// </summary>
[DefaultExecutionOrder(10000)] // Execute last so that camera accounts for any geometry intersection fixes
public class CameraFollower : MonoBehaviour
{
    /// <summary>
    /// A reference to the target.
    /// </summary>
    public GameObject target;
    /// <summary>
    /// An offset to the targets rotaiton in degrees
    /// </summary>
    public Vector3 rotationOffset;
    /// <summary>
    /// An offset to the targets position
    /// </summary>
    public Vector3 positionOffset;
    /// <summary>
    /// Sets the position and rotation of this GameObject to be the same as target
    /// </summary>
    void LateUpdate()
    {   //Make sure we have a target
        if (target == null)
            return;

        var pc = target.GetComponent<PlayerController>();
        //Set the cameras target position and rotation
        transform.position = target.transform.position + positionOffset;
        Vector3 v = target.transform.eulerAngles + rotationOffset;
        v.x = pc.xEulerRotation;
        transform.eulerAngles = v;
    }
}
