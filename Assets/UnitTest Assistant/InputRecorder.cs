using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace InputRecording.NewInputSystem
{
    [CreateAssetMenu(fileName = "Recorder", menuName = "Input Recorder", order = 0)]
    public class InputRecorder : ScriptableObject
    {
        /// <summary>
        /// The valid gamepad buttons to track
        /// </summary>
        private static readonly GamepadButton[] s_recordedGamepadButtons = new GamepadButton[] { GamepadButton.North, GamepadButton.South, GamepadButton.East, GamepadButton.West, GamepadButton.Start, GamepadButton.Select, GamepadButton.LeftShoulder, GamepadButton.RightShoulder, GamepadButton.LeftTrigger, GamepadButton.RightTrigger, GamepadButton.LeftStick, GamepadButton.RightStick, GamepadButton.DpadUp, GamepadButton.DpadDown, GamepadButton.DpadLeft, GamepadButton.DpadRight };
        /// <summary>
        /// The StreamWriter for writing to the file
        /// </summary>
        private StreamWriter _fileWriter = null;
        /// <summary>
        /// The time when the input recording started. This to account for the time before the StartRecording button was pressed
        /// </summary>
        private float _startTime = 0;
        /// <summary>
        /// The path to the file
        /// </summary>
        [Tooltip("The path and name of the file. It should be appended with .txt")]
        public string m_filePathWithName = "";
        /// <summary>
        /// Starts recording the inputs
        /// </summary>
        public void StartRecording()
        {
            try
            {   //Attempt to open a stream writer
                _fileWriter = new StreamWriter(m_filePathWithName);

                _startTime = Time.time;
                //If it was successful, subscribe to the input system event
                InputSystem.onEvent += RecordEvent;
            }//Otherwise log an error
            catch (System.Exception e) { Debug.LogError("Failed to open StreamWriter to given file. " + e.ToString()); }
        }
        /// <summary>
        /// Records an input from a device
        /// </summary>
        /// <param name="eventPtr"></param>
        /// <param name="device"></param>
        private void RecordEvent(InputEventPtr eventPtr, InputDevice device)
        {   //Make sure the eventPtr is a stateEvent                                                                                                                                            
            if ((!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) || _fileWriter == null)
                return;
            //Check if the device is a gamepad
            if (device is Gamepad gamepad)
            {   //If so, record its inputs
                //Inputs can be read using gamepad.(button).ReadValue();
                //Write the time and the ID of the device this is from
                string inputs = (eventPtr.time - _startTime).ToString() + "|" + device.deviceId + "|";
                //Loop over the buttons
                float value;
                foreach (GamepadButton button in s_recordedGamepadButtons)
                {
                    try
                    {   //Attempt to read the value. This will throw an exception if the button is invalid. Eg: A, B, X, Y will throw exceptions. North, South, East, West won't
                        value = gamepad[button].ReadValue();
                        //Write it to the input line, being buttonName[value]|buttonName[value]
                        inputs += button.ToString() + "[" + value + "]|";
                    }
                    catch { value = 0; Debug.LogError("Failed to read input from " + button.ToString()); }
                    //Store the value of the button
                }
                //Read the values for the thumbsticks
                Vector2 axisValue = gamepad.leftStick.ReadValue();
                //Store the value
                //We don't use axisValue.ToString() because I'm not entirely sure what format it uses
                inputs += "LStick[" + axisValue.x + "," + axisValue.y + "]|";
                //Repeat for right stick
                axisValue = gamepad.rightStick.ReadValue();
                inputs += "RStick[" + axisValue.x + "," + axisValue.y + "]|";
                //Write the line to the file
                _fileWriter.WriteLine(inputs);
            }

            if (device is Keyboard keyboard)
            {
                string inputs = (eventPtr.time - _startTime).ToString() + "|" + device.deviceId + "|";

                inputs += "A[" + keyboard.aKey.ReadValue() + "]|";
                //Write the line to the file
                _fileWriter.WriteLine(inputs);
            }
        }
        /// <summary>
        /// Stops recording the inputs
        /// </summary>
        public void StopRecording()
        {
            InputSystem.onEvent -= RecordEvent;
            //Save data to text file
            _fileWriter.Close();
            _fileWriter = null;
        }
    }
}