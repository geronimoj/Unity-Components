using System.Collections.Generic;
using UnityEngine;
using FInput;
using System.IO;
using System;

namespace Replay
{
    /// <summary>
    /// For replaying a series of inputs
    /// </summary>
    //Make sure it runs earlier than normal scripts so it has time to update the inputs
    [DefaultExecutionOrder(-100)]
    public class InputReplayer : MonoBehaviour
    {
        /// <summary>
        /// The inputs for each line in string form
        /// </summary>
        private readonly List<string> _inputs = new List<string>();
        /// <summary>
        /// We store this as a dictionary unlike the KeyCodes because we only need one of them
        /// </summary>
        private readonly Dictionary<string, float> _axisValues = new Dictionary<string, float>();
        /// <summary>
        /// The path to the .txt file that contains the replay
        /// </summary>
        [Tooltip("The path to the .txt file that contains the replay")]
        public string m_filePath = "";
        /// <summary>
        /// Is a replay currently playing. IS READ ONLY
        /// </summary>
        [SerializeField]
        [Tooltip("Is a replay currently playing")]
        private bool _isPlaying = false;
        /// <summary>
        /// Is a replay currently playing
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            internal set
            {
                _isPlaying = value;
            }
        }
        /// <summary>
        /// Starts the replay from the beginning
        /// </summary>
        [ContextMenu("Start Playing")]
        public void StartReplay()
        {   //If its playing, do nothing
            if (IsPlaying)
                return;
            try
            {   //Attempt to open a new stream reader
                StreamReader sr = new StreamReader(m_filePath);
                //Store the entire file in a list
                while (!sr.EndOfStream)
                    //If we have not reached the end of the stream, read the next line
                    _inputs.Add(sr.ReadLine());

                Debug.Log("Replay successfully loaded");
            }
            catch (Exception e)
            {   //Failed so log an error and return
                Debug.LogError("Failed to find and load the replay: " + e.ToString());
                return;
            }

            InputGetter.ReplayedAxisValues = _axisValues;
            //We successfully loaded the inputs
            _isPlaying = true;
        }
        /// <summary>
        /// Stops the replay
        /// </summary>
        [ContextMenu("Stop Playing")]
        public void StopReplay()
        {   //If its not playing, do nothing
            if (!IsPlaying)
                return;
            //Clear all the data
            IsPlaying = false;
            _inputs.Clear();
            _axisValues.Clear();
            InputGetter.ReplayedAxisValues = null;
            InputGetter.ReplayedInputs = null;
        }
        /// <summary>
        /// Moves the replay along
        /// </summary>
        private void FixedUpdate()
        {   //Only bother if we are playing
            if (!IsPlaying)
                return;
            //We decifer each line on a as need basis since its not too expensive
            //It also saves a bit of load time.
            //Convert it to a string
            string[] inputs = _inputs[0].Split(':');
            //Create a new list to store the inputs. Not efficient since this will be done every frame but its ok for now I suppose
            //This is designed for debugging after all and doesn't have to be extrememly efficient.
            List<KeyCode> keys = new List<KeyCode>();
            //We ignore the first part as that is just the FixedUpdate step that is was part of
            //The first step is identifying axis vs keycodes. 
            //If my memory serves me correctly an axis appears like 'AxisName'Value a KeyCode appears like 'KeyCode'
            string[] line;
            for (int i = 1; i < inputs.Length; i++)
            {   //We have to do this stupid convert thing because ' can't be gotten using '''
                line = inputs[i].Split(Convert.ToChar("'"));
                //If there was only 1, then it was a keycode
                if (line.Length == 1)
                {
                    //Attempt to read it as a KeyCode
                    if (Enum.TryParse<KeyCode>(line[0], out KeyCode code))
                    {
                        //If it was successful store the value
                        keys.Add(code);
                        continue;
                    }
                    //Log an error because it failed and return
                    Debug.LogError("Failed to decifer Keycode from string");
                    return;
                }
                //Otherwise it is an axis
                try
                {   //Attempt to convert the second half into a float
                    _axisValues[line[0]] = float.Parse(line[1]);
                }
                catch 
                {   //Log an error and return
                    Debug.LogError("Failed to convert the line from the axis");
                    return;
                }
            }
            //Update the Input in the InputGetter
            InputGetter.ReplayedInputs = keys;
            //Remove the first input
            _inputs.RemoveAt(0);
        }
    }
}