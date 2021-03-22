using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A struct for storing movement information with corresponding Get Sets.
/// </summary>
[System.Serializable]
public struct MovementDirection
{
    /// <summary>
    /// A storage location for our movement vector
    /// </summary>
    private Vector3 vec;
    /// <summary>
    /// A storage location for the direction of movement
    /// </summary>
    private Vector3 dir;
    /// <summary>
    /// A storage location for the maximum Hozizontal Speed
    /// </summary>
    [SerializeField]
    [Tooltip("The maximum horizontal speed of the player")]
    private float maxHoz;
    /// <summary>
    /// A storage location for the maximum Vertical Speed
    /// </summary>
    [Tooltip("The maximum vertical speed of the player")]
    [SerializeField]
    private float maxVert;
    /// <summary>
    /// Returns or sets the horizontal component of the vector.
    /// This will be clamped by MaxHozSpeed if it is not 0.
    /// Can be positive or negative.
    /// Has no influence over the direction of movement
    /// </summary>
    public float HozSpeed
    {
        get
        {
            return Vector3.Dot(dir, new Vector3(vec.x, 0, vec.z));
        }
        set
        {   
            //Make sure value is within range & maxHoz is limited
            if (maxHoz != 0)
            {
                if (value > maxHoz)
                    value = maxHoz;
                if (value < -maxHoz)
                    value = -maxHoz;
            }
            //Set the horizontal speed
            Vector3 hozDir = HozDirection;
            vec.x = value * hozDir.x;
            vec.z = value * hozDir.z;
        }
    }
    /// <summary>
    /// Returns or sets the vertical component of the vector.
    /// This will be clamped by MaxVertSpeed if its not 0.
    /// Can be positive or negative.
    /// Has influence over the direction of movement.
    /// </summary>
    public float VertSpeed
    {
        get
        {
            return vec.y;
        }
        set
        {   //Check if the maximum vertical speed was capped
            if (maxVert != 0)
            {   //Clamp the set value between positive and negative max vertical speed
                if (value > maxVert)
                    value = maxVert;
                if (value < -maxVert)
                    value = -maxVert;
            }
            //Set the vertical speed
            vec.y = value;
            //Re-calculate the direction of movement
            Direction = vec.normalized;
        }
    }
    /// <summary>
    /// Returns the length of the move vector.
    /// Sets the length of the move vector.
    /// </summary>
    public float TotalSpeed
    {
        get
        {
            return vec.magnitude;
        }
        set
        {   //Make sure total max speed has been limited
            if (TotalMaxSpeed != 0)
                //Make sure value does not exceed the max speed
                if (value > TotalMaxSpeed)
                    value = TotalMaxSpeed;
            //Set the total speed
            vec = dir * value;
        }
    }
    /// <summary>
    /// Returns the absolute maximum horizontal speed.
    /// Sets the maximum horizontal speed & clamps.
    /// Set to 0 for no clamp
    /// </summary>
    public float MaxHozSpeed
    {
        get
        {
            return maxHoz;
        }
        set
        {   //Make sure the value is not less than 0
            if (value <= 0)
            {
                maxHoz = 0;
                return;
            }
            //Assign the max horizontal speed
            maxHoz = value;
            //Clamp the current horizontal speed if it now exceeds the new max speed
            if (HozSpeed > maxHoz)
                HozSpeed = maxHoz;
            if (HozSpeed < -maxHoz)
                HozSpeed = -maxHoz;
        }
    }
    /// <summary>
    /// Returns the absolute maximum vertical speed.
    /// Sets the maximum vertical speed & clamps.
    /// Set to 0 for no clamp.
    /// </summary>
    public float MaxVertSpeed
    {
        get
        {
            return maxVert;
        }
        set
        {   //Make sure the maximum vertical speed is positive or 0
            if (value <= 0)
            {
                maxVert = 0;
                return;
            }
            //Assign the maximum vertical speed
            maxVert = value;
            //Clamp the current vertical speed by the new maximum vertical speed
            if (VertSpeed > maxVert)
                VertSpeed = maxVert;
            if (VertSpeed < -maxVert)
                VertSpeed = -maxVert;
        }
    }
    /// <summary>
    /// Returns the total absolute maximum speed
    /// </summary>
    public float TotalMaxSpeed
    {
        get
        {
            return Mathf.Sqrt((maxHoz * maxHoz) + (maxVert * maxVert));
        }
    }
    /// <summary>
    /// Returns the forward direction of movement. (Assumes HozSpeed >= 0)
    /// Sets the direction of the movementDirection.
    /// Use TrueDirection to get the direction of movement reguardless of HozSpeed.
    /// </summary>
    public Vector3 Direction
    {
        get
        {
            return dir;
        }
        set
        {   //Make sure the new direction is not zero
            if (value == Vector3.zero)
                return;
            //Assign the direction
            dir = value.normalized;
            //If HozSpeed is < 0, then we need to flip the horizontal component to retain the direction
            vec = dir * vec.magnitude;

            if (HozSpeed < 0)
            {
                vec.x *= -1;
                vec.z *= -1;
            }
            //Make sure the vector remains clamped
            ClampVector();
        }
    }
    /// <summary>
    /// Returns the current direction of movement.
    /// This is unaffected by HozSpeed being > or < 0.
    /// </summary>
    public Vector3 TrueDirection => vec.normalized;
    /// <summary>
    /// Returns a normalised vector representing the forward horizontal (x & z) direction of movement. (Assumes HozSpeed >= 0)
    /// Sets the forward horizontal direction of movement. Normalizes vector automatically.
    /// Forwards means: The direction if hozSpeed > 0. If hozSpeed is < 0, the direction will be -forwards.
    /// Use TrueHozDirection to get the horizontal direction of movement reguardless of HozSpeed.
    /// </summary>
    public Vector3 HozDirection
    {
        get
        {
            return new Vector3(dir.x, 0, dir.z).normalized;
        }
        set
        {   //Make sure the new horizontal direction is not 0
            if (value == Vector3.zero)
                return;
            //Set the y to zero so as keep everything normalized
            value.y = 0;
            //Since the magnitude of hoz or vert speed isn't changing
            //All we have to do is retain the magnitude of the horizontal component to change the direction. This will result in the new direction still being a unit vector
                 //Calculate the horizontal component                         //Retain the y component
            dir = (value.normalized * new Vector3(dir.x, 0, dir.z).magnitude) + new Vector3(0, dir.y, 0);
            //Scale it by speed
            value = value.normalized * HozSpeed;
            //Set vec's x & z components
            vec.x = value.x;
            vec.z = value.z;
        }
    }
    /// <summary>
    /// Returns the current horizontal direction of movement.
    /// This is unaffected by HozSpeed being > or < 0.
    /// </summary>
    public Vector3 TrueHozDirection => new Vector3(vec.x, 0, vec.z).normalized;
    /// <summary>
    /// Returns the movement vector with direction and magnitude.
    /// Is Read Only.
    /// </summary>
    public Vector3 TotalVector
    {
        get
        {
            return vec;
        }
    }
    /// <summary>
    /// Transitions the direction of this MD to the direction of final
    /// </summary>
    /// <param name="final">The target rotation</param>
    /// <param name="time">The duration of the transition</param>
    /// <param name="timer">A storage location for the timer. Expected to be <= 0 the first time this function is called</param>
    public void SmoothDirection(Vector3 final, float time, ref float timer)
    {
        //Ensure final is normalised
        if (final.magnitude != 1)
            final.Normalize();
        //Check if this is a new smooth
        if (timer <= 0 || timer > time)
            timer = time;
        //Store time so its consistent across all calculations
        float t = Time.deltaTime;
        //If we are going to do the last bit this frame, might as well just TP to the target.
        if (t > timer)
        {
            Direction = final;
            timer = 0;
            return;
        }
        //Calculate how much dif represents the full change
        float per = timer/time;
        //Calculate how much percent we will move this frame.
        float fPer = t / time;
        //Calculate the difference in direction
        Vector3 dif = final - Direction;
        //Adjust the length of the dif vector by how much we are going to move this frame
        dif *= fPer / per;
        //Apply the change.
        Direction += dif;
        //Decrement the timer
        timer -= t;
    }
    /// <summary>
    /// Transitions the direction of this MD to the direction of final
    /// </summary>
    /// <param name="final">The target rotation</param>
    /// <param name="velocity">The speed of the rotation</param>
    public void SmoothDirection(Vector3 final, float velocity)
    {//Ensure final is normalised
        if (final.magnitude != 1)
            final.Normalize();
        //If velocity is 0 or less, we can't smooth to the direction
        if (velocity <= 0)
            return;
        //Get the difference
        Vector3 dif = final - Direction;
        //Check if we should teleport to the angle or we step
        if (velocity * Time.deltaTime < dif.magnitude)
            Direction += dif.normalized * velocity * Time.deltaTime;
        else
            //The velocity change is too big so teleport to the target direction
            Direction = final;
    }
    /// <summary>
    /// Clamps the horizontal and vertical components of the move vector
    /// </summary>
    private void ClampVector()
    {
        float value = VertSpeed;
        if (maxVert != 0)
        {   //Clamp the set value between positive and negative max vertical speed
            if (value > maxVert)
                VertSpeed = maxVert;
            if (value < -maxVert)
                VertSpeed = -maxVert;
        }
        value = HozSpeed;
        //Repeat for horizontal
        if (maxHoz != 0)
        {
            if (value > maxHoz)
                HozSpeed = maxHoz;
            if (value < -maxHoz)
                HozSpeed = -maxHoz;
        }
    }
}
