using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;

namespace FInput
{
    /// <summary>
    /// Acts as a middle man for inputs for the InputReplayer
    /// </summary>
    public class InputGetter : MonoBehaviour
    {
        /// <summary>
        /// Which inputs were pressed this frame. This is used for forcing inputs for the InputReplayer
        /// </summary>
        public static ReadOnlyCollection<KeyCode> ReplayedInputs { set; internal get; } = null;
        /// <summary>
        /// The values for the axis that were read from the InputReplayer.
        /// </summary>
        public static ReadOnlyDictionary<string, float> ReplayedAxisValues { set; internal get; } = null;
        /// <summary>
        /// Returns true if any key has been pressed. Will priorities reading from a replayed input if one is playing.
        /// </summary>
        public static bool anyKey
        {
            get
            {   //Check if there is a replay
                if (ReplayedInputs != null)
                {   //If so check for any inputs
                    if (ReplayedInputs.Count > 0)
                        //If there is at least 1, return true
                        return true;
                    //Otherwise false
                    return false;
                }
                //If there is no replay, return normal Input.anyKey
                return Input.anyKey;
            }
        }
        /// <summary>
        /// Returns true the first frame any key was pressed down. Will priorities reading from a replayed input if one is playing.
        /// </summary>
        public static bool anyKeyDown
        {
            get
            {
                return Input.anyKeyDown;
            }
        }
        /// <summary>
        /// Returns the value of the axis. This will return the axis from a replay if one is being played.
        /// </summary>
        /// <param name="axis">The name of the axis</param>
        /// <returns>The value of the axis</returns>
        public static float GetAxis(string axis)
        {   //Make sure there are axis values to read
            if (ReplayedAxisValues != null 
                //Check if the axis is contained
                && ReplayedAxisValues.ContainsKey(axis))
                //If so, return the axis from the replay
                return ReplayedAxisValues[axis];
            //Otherwise return Input.GetAxis
            return Input.GetAxis(axis);
        }
        /// <summary>
        /// Returns the value of the axis as 1, -1 or 0. This will return the axis from a replay if one is being played.
        /// </summary>
        /// <param name="axis">The name of the axis</param>
        /// <returns>The value of the axis rounded to the nearest int</returns>
        public static float GetAxisRaw(string axis)
        {   //Make sure there are axis values to read
            if (ReplayedAxisValues != null
                //Check if the axis is contained
                && ReplayedAxisValues.ContainsKey(axis))
            {
                //If so, return the axis from the replay rounded to 1 or -1
                float val = ReplayedAxisValues[axis];
                //Check 0 first since it could be the most common
                if (val == 0)
                    return 0;
                //Check positive and negative
                if (val > 0)
                    return 1;
                if (val < 0)
                    return -1;
            }
            //Otherwise return Input.GetAxis
            return Input.GetAxisRaw(axis);
        }
    }
}
