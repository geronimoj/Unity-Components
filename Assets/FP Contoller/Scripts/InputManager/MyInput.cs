using UnityEngine;

/// <summary>
/// A custom class for representing Inputs in the InputManager
/// </summary>
[System.Serializable]
public class MyInput
{
    /// <summary>
    /// The name of the input
    /// </summary>
    [Tooltip("The name used for searching for this input")]
    public string name = "NULL";
#if UNITY_STANDALONE_WIN
    /// <summary>
    /// The Axis that this input represents from Input.GetAxis
    /// name variable will be used if this is left as "NULL"
    /// </summary>
    [Tooltip("The corresponding Unity Axis. Defaults to name if NULL")]
    public string axisName = "NULL";
#endif
    /// <summary>
    /// A copy of the previous value. Used for checking for a new input
    /// </summary>
    private float previousValue = 0;
    /// <summary>
    /// The current value of this input
    /// </summary>
    private float value = 0;
#if UNITY_ANDROID
    /// <summary>
    /// Used by buttons to determine if they were pressed this frame
    /// </summary>
    private bool pressed = false;
#endif
    /// <summary>
    /// Returns the value of this input
    /// If in android, sets the value of the axis
    /// </summary>
    public float GetValue
    {
        get
        {
            return value;
        }
#if UNITY_ANDROID
        set
        {
            pressed = true;
            previousValue = this.value;
            this.value = value;
        }
#endif
    }

    /// <summary>
    /// Returns true if this was the first input this frame
    /// </summary>
    public bool PressedThisFrame()
    {   //If the previous value was 0, we assume that this is a new input
        return previousValue == 0;
    }

    /// <summary>
    /// Updates the inputs previousValue.
    /// If in a Windows build, also updates value based on Input.GetAxis()
    /// </summary>
    public void UpdateInput()
    {
#if UNITY_ANDROID
        //This is done for buttons as they only call SetValue on press and release, not the inbetween.
        if (!pressed)
            //Store the previous values data
            previousValue = value;
        else
            pressed = false;
#endif
#if UNITY_STANDALONE_WIN
        //Store the previous values data
        previousValue = value;
        //Update the input
        if (axisName != "NULL")
            value = Input.GetAxis(axisName);
        //A backup just in case axisName is null. This occurs after swapping between Android & Windows mode
        else
            value = Input.GetAxis(name);

#endif
    }
}
