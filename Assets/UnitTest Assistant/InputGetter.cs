using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace FInput
{
    /// <summary>
    /// Acts as a middle man for inputs for the InputReplayer.
    /// The InputReplayer code is not cut using defines so this can be used for a replay system.
    /// </summary>
    public class InputGetter
    {
        /// <summary>
        /// The keyCodes from the last input update
        /// </summary>
        private static List<KeyCode> _lastFrameInputs = null;
        /// <summary>
        /// The keycodes from the current input update
        /// </summary>
        private static List<KeyCode> _thisFrameInputs = null;
        /// <summary>
        /// Which inputs were pressed this frame. This is used for forcing inputs for the InputReplayer
        /// </summary>
        public static List<KeyCode> ReplayedInputs
        {
            set
            {   //If value is null, for get all inputs for this indicates the end of a replay
                if (value == null)
                {
                    _lastFrameInputs = null;
                    _thisFrameInputs = null;
                    return;
                }
                
                //Store the last updates inputs
                _lastFrameInputs = _thisFrameInputs;
                //Set this frames updates
                _thisFrameInputs = value;
            }
        }
        /// <summary>
        /// The values for the axis that were read from the InputReplayer.
        /// </summary>
        public static ReadOnlyDictionary<string, float> ReplayedAxisValues { set; internal get; } = null;
        /// <summary>
        /// Returns true if any key has been pressed. Will priorities reading from a replayed input if one is playing.
        /// </summary>
        //Yes, VS calls this a naming rule violation but we want it to be identical to Unitys Input
        public static bool anyKey
        {
            get
            {   //Check if there is a replay
                if (_thisFrameInputs != null)
                {   //If so check for any inputs
                    if (_thisFrameInputs.Count > 0)
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
        //Yes, VS calls this a naming rule violation but we want it to be identical to Unitys Input
        public static bool anyKeyDown
        {
            get
            {
                //Compare last frame and this frames inputs. If there is a difference, return true;
                //Null check
                if (_thisFrameInputs != null)
                {   //Quick check that there are inputs this frame
                    if (_thisFrameInputs.Count == 0)
                        return false;
                    //If there is no last frame buffer, then these inputs are the new inputs
                    if (_lastFrameInputs == null || _lastFrameInputs.Count == 0)
                        return true;
                    //With the two easiest checks out of the way, 
                    //we need to check if thisFrameInputs contains a new input
                    foreach(KeyCode key in _thisFrameInputs)
                    {   //Check if the last frames inputs contained the keyCode
                        if (!_lastFrameInputs.Contains(key))
                            //If not, we got a new input so return true
                            return true;
                    }
                    //If we went through the entire loop without the a new input, return false
                    return false;
                }
                //Default to Input.anyKeyDown
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

        public static bool GetKeyDown(KeyCode key)
        {   //Default to unity GetKey
            return Input.GetKeyDown(key);
        }

        public static bool GetKey(KeyCode key)
        {   //Default to unity GetKey
            return Input.GetKey(key);
        }

        public static bool GetKeyUp(KeyCode key)
        {   //Default to unity GetKey
            return Input.GetKeyUp(key);
        }
    }
}
