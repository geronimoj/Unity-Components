using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace InputRecording.NewInputSystem
{
    public class InputRecorder : ScriptableObject
    {
        private static readonly GamepadButton[] s_recordedGamepadButtons = new GamepadButton[] { GamepadButton.North, GamepadButton.South, GamepadButton.East, GamepadButton.West, GamepadButton.Start, GamepadButton.Select, GamepadButton.LeftShoulder, GamepadButton.RightShoulder, GamepadButton.LeftTrigger, GamepadButton.RightTrigger, GamepadButton.LeftStick, GamepadButton.RightStick, GamepadButton.DpadUp, GamepadButton.DpadDown, GamepadButton.DpadLeft, GamepadButton.DpadRight };

        public void StartRecording()
        {
            InputSystem.onEvent += RecordEvent;
        }

        private void RecordEvent(InputEventPtr eventPtr, InputDevice device)
        {   //Make sure the eventPtr is a stateEvent                                                                                                                                            
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            //Check if the device is a gamepad
            if (device is Gamepad gamepad)
            {   //If so, record its inputs
                //Inputs can be read using gamepad.(button).ReadValue();
                //Loop over the buttons
                float value;
                foreach (GamepadButton button in s_recordedGamepadButtons)
                {
                    try
                    {   //Attempt to read the value. This will throw an exception if the button is invalid. Eg: A, B, X, Y will throw exceptions. North, South, East, West won't
                        value = gamepad[button].ReadValue();
                    }
                    catch { value = 0; Debug.LogError("Failed to read input from " + button.ToString()); }
                    //Store the value of the button
                }

                //Read the values for the thumbsticks
                Vector2 axisValue = gamepad.leftStick.ReadValue();
                //Store the value

                //Repeat for right stick
            }
        }

        public void StopRecording()
        {
            InputSystem.onEvent -= RecordEvent;
        }
    }
}