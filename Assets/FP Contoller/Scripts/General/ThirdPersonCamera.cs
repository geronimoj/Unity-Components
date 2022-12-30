using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public Vector3 positionOffset;

    void Update()
    {   //Null catch
        if (!target)
            return;

        Vector3 newPos = target.position + target.forward * positionOffset.z + target.right * positionOffset.x + Vector3.up * positionOffset.y;

        Vector3 toCam = newPos - target.position;

        if (Physics.Raycast(target.position, toCam.normalized, out RaycastHit hit, toCam.magnitude))
            newPos = target.position + toCam.normalized * hit.distance;

        transform.position = newPos;

        transform.rotation = target.rotation;
    }
}
