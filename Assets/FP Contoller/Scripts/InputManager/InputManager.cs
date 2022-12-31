using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A custom InputManager class made to create consistency between Android & Windows builds.
/// Attach to a GameObject that will never be destroyed.
/// </summary>
public class InputManager : MonoBehaviour
{
    /// <summary>
    /// An array of all inputs. This is moved to the static inputs array upon starting the project
    /// </summary>
    [Tooltip("An array of inputs that can be checked. Cannot be changed during runtime")]
    public MyInput[] Inputs;
    /// <summary>
    /// The inputs but static so they can be read globally
    /// </summary>
    public static MyInput[] inputs;
    /// <summary>
    /// Moves the inputs to be static upon launching the project
    /// </summary>
    public void Awake()
    {   //Copy over the inputs into a static variable
        inputs = Inputs;
        Debug.Log("Loading inputs");
    }

    /// <summary>
    /// Searches and returns the value of the input with a name of name
    /// </summary>
    /// <param name="name">The name of the input</param>
    /// <returns>Returns 0 and logs an error if the input could not be found. Otherwise returns the value of the input</returns>
    public static float GetInput(string name)
    {   if (inputs == null)
            return Input.GetAxis(name);
        //Loop through the inputs and search for the correct input
        for (int i = 0; i < inputs.Length; i++)
            if (inputs[i].name == name)
                //Return the value of the input
                return inputs[i].GetValue;

        //Return 0 if we don't find an input
        Debug.LogError("Invalid input name: " + name);
        return 0;
    }

    /// <summary>
    /// Returns the value of the input if the input was only just pressed
    /// </summary>
    /// <param name="name">The name of the input</param>
    /// <returns>Returns 0 if the input could not be found</returns>
    public static float NewInput(string name)
    {   if (inputs == null)
            return 0;
        //Loop through the inputs and search for the correct input
        for (int i = 0; i < inputs.Length; i++)
            if (inputs[i].name == name)
            {   //Was the input pressed this frame
                if (inputs[i].PressedThisFrame())
                    //Return the value of the input
                    return inputs[i].GetValue;
                else
                    //Return 0
                    return 0;
            }
        //Return 0 if we can't find a valid input
        Debug.LogError("Invalid input name: " + name);
        return 0;
    }

#if UNITY_ANDROID
    /// <summary>
    /// Attempts to set the value of the input
    /// Does nothing if the input cannot be found
    /// </summary>
    /// <param name="name">The name of the input</param>
    /// <param name="val">The value to be set</param>
    public static void SetInput(string name, float val)
    {   //Find the corresponding index
        for (int i = 0; i < inputs.Length; i++)
            if (inputs[i].name == name)
                //Set the value
                SetInput(i, val);
    }

    /// <summary>
    /// A storage location for SetValue(float)
    /// </summary>
    private float val = 0;
    /// <summary>
    /// Sets the value of the input with name to the value of SetValue(float)
    /// </summary>
    /// <param name="name">The name of the input</param>
    public void SetInputWrapper(string name)
    {
        SetInput(name, val);
    }
    /// <summary>
    /// Event Triggers only allow 1 parameter. Call this before calling SetInputWrapper to set the value of the input
    /// </summary>
    /// <param name="v">The value to set the input of</param>
    public void SetValue(float v)
    {
        val = v;
    }
    /// <summary>
    /// Sets the value of an input. index version. Only used with On Screen controls.
    /// </summary>
    /// <param name="index">The index of the input</param>
    /// <param name="val">The value to set to input</param>
    public static void SetInput(int index, float val)
    {
        inputs[index].SetValue(val);
    }
#endif
    /// <summary>
    /// Calls update on all of the inputs to keep them up to date
    /// </summary>
    void Update()
    {   //Call update on all of the inputs
        for (int i = 0; i < inputs.Length; i++)
            inputs[i].UpdateInput();
    }
}
