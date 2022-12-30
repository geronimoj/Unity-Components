using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomController;
/// <summary>
/// Used to rotate ColliderInfo based on time and the given values
/// </summary>
[CreateAssetMenu(fileName = "HitboxAnimation", menuName = "Hitbox Animation", order = 0)]
public class HitboxAnimation : ScriptableObject
{
    /// <summary>
    /// The information used to rotate the ColliderInfo
    /// </summary>
    public KeyframeInfo[] keyframeInfo;
    /// <summary>
    /// Changes the information in the given ColliderInfo based on how much time has passed and the information given by rotationInformation.
    /// Timer is automatically reset and incremented.
    /// </summary>
    /// <param name="c">The colliderInfo to change</param>
    /// <param name="timer">A reference to a timer</param>
    public void AnimateColliderInfo(ref CapsualCollider c, ref float timer)
    {   //If there is no rotation information, just exit
        if (keyframeInfo == null || keyframeInfo.Length == 0)
        {
            timer = 0;
            return;
        }
        float duration = 0;
        //Icrement the timer
        timer += Time.deltaTime;
        //Loop through the rotationInfromation
        for (int i = 0; i < keyframeInfo.Length; i++)
        {   //Increase duration by each rotationInformation.
            duration += keyframeInfo[i].duration;
            //If timer is <= duration then the current keyframe will be this one
            if (timer < duration)
            {   //Check if we want to lerp and if we can lerp to the next keyframe.
                if (keyframeInfo[i].lerpToNext && i < keyframeInfo.Length - 1)
                {
                    //We re-use duration for storing our step percent for memory efficiency since we don't need it anymore
                    duration -= keyframeInfo[i].duration;
                    //Get the percentage through this keyframe.
                    duration = (timer - duration) / keyframeInfo[i].duration;

                    #region Assigning KeyframeInfo
                    //Is lowerHeight being ignored?
                    if (!keyframeInfo[i].ignoreLowerHeight)
                        c.LowerHeight = Mathf.Lerp(keyframeInfo[i].lowerHeight, keyframeInfo[i + 1].lowerHeight, duration);
                    //Is upperHeight being ignored?
                    if (!keyframeInfo[i].ignoreUpperHeight)
                        c.UpperHeight = Mathf.Lerp(keyframeInfo[i].upperHeight, keyframeInfo[i + 1].upperHeight, duration);
                    //Is radius being ignored?
                    if (!keyframeInfo[i].ignoreRadius)
                        c.Radius = Mathf.Lerp(keyframeInfo[i].radius, keyframeInfo[i + 1].radius, duration);
                    //Is orientation being ignored?
                    if (!keyframeInfo[i].ignoreOrientation)
                        c.Orientation = Vector3.Lerp(keyframeInfo[i].orientation, keyframeInfo[i + 1].orientation, duration).normalized;
                    //Is the positional offset being ignored?
                    if (!keyframeInfo[i].ignorePositionOffset)
                        c.PositionOffset = Vector3.Lerp(keyframeInfo[i].posOffset, keyframeInfo[i + 1].posOffset, duration);
                    #endregion
                }
                else
                {
                    #region Assigning KeyframeInfo
                    //Is lowerHeight being ignored?
                    if (!keyframeInfo[i].ignoreLowerHeight)
                        c.LowerHeight = keyframeInfo[i].lowerHeight;
                    //Is upperHeight being ignored?
                    if (!keyframeInfo[i].ignoreUpperHeight)
                        c.UpperHeight = keyframeInfo[i].upperHeight;
                    //Is radius being ignored?
                    if (!keyframeInfo[i].ignoreRadius)
                        c.Radius = keyframeInfo[i].radius;
                    //Is orientation being ignored?
                    if (!keyframeInfo[i].ignoreOrientation)
                        c.Orientation = keyframeInfo[i].orientation.normalized;
                    //Is the positional offset being ignored?
                    if (!keyframeInfo[i].ignorePositionOffset)
                        c.PositionOffset = keyframeInfo[i].posOffset;
                    #endregion
                }
                //Since we have found the current keyframe, exit.
                break;
            }
            //If we have reached the last keyframe & are not in range for it. Reset the timer & exit
            else if (i == keyframeInfo.Length - 1)
            {
                timer = 0;

                #region Assigning KeyframeInfo
                //Is lowerHeight being ignored?
                if (!keyframeInfo[i].ignoreLowerHeight)
                    c.LowerHeight = keyframeInfo[i].lowerHeight;
                //Is upperHeight being ignored?
                if (!keyframeInfo[i].ignoreUpperHeight)
                    c.UpperHeight = keyframeInfo[i].upperHeight;
                //Is radius being ignored?
                if (!keyframeInfo[i].ignoreRadius)
                    c.Radius = keyframeInfo[i].radius;
                //Is orientation being ignored?
                if (!keyframeInfo[i].ignoreOrientation)
                    c.Orientation = keyframeInfo[i].orientation.normalized;
                //Is the positional offset being ignored?
                if (!keyframeInfo[i].ignorePositionOffset)
                    c.PositionOffset = keyframeInfo[i].posOffset;
                #endregion

                return;
            }
        }
    }
}

public struct KeyframeInfo
{
    /// <summary>
    /// The duration this "stage" lasts
    /// </summary>
    [Tooltip("How long is waited until transitoning to the next rotation")]
    public float duration;
    /// <summary>
    /// If true, we should lerp values to the next stage if possible over the course of duration
    /// </summary>
    [Tooltip("Determines if we should transition to the next rotation over the course of this instance.")]
    public bool lerpToNext;
    /// <summary>
    /// Should we change the lowerHeight
    /// </summary>
    [Tooltip("Set to true if lowerHeight should be ignored for this stage")]
    public bool ignoreLowerHeight;
    /// <summary>
    /// The lower height to change to
    /// </summary>
    [Tooltip("How far below the origin the capsual extends. Includes radius.")]
    public float lowerHeight;
    /// <summary>
    /// Should we change the upper height
    /// </summary>
    [Tooltip("Set to true if upper height should be ignored for this stage")]
    public bool ignoreUpperHeight;
    /// <summary>
    /// The upper height to change to
    /// </summary>
    [Tooltip("How far above the origin the capsual extends. Includes radius")]
    public float upperHeight;
    /// <summary>
    /// Should we change the radius
    /// </summary>
    [Tooltip("Set to true if radius should be ignored for this stage")]
    public bool ignoreRadius;
    /// <summary>
    /// The radius to change to
    /// </summary>
    [Tooltip("The radius of the capsual")]
    public float radius;
    /// <summary>
    /// Should we change the orientation
    /// </summary>
    [Tooltip("Set to true if orientation should be ignored for this stage")]
    public bool ignoreOrientation;
    /// <summary>
    /// The orientation to change to
    /// </summary>
    [Tooltip("The rotation of the collider relative to the origins transform. Represents the up direction of the collider")]
    public Vector3 orientation;
    /// <summary>
    /// Should the position offset be changed
    /// </summary>
    [Tooltip("Should the positionOffset be changed")]
    public bool ignorePositionOffset;
    /// <summary>
    /// The positional offset of the collider from its origin
    /// </summary>
    [Tooltip("The positional offset of the collider from its origin. Is relative to the origins rotation")]
    public Vector3 posOffset;
}