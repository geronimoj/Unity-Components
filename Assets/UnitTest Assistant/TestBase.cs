using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

namespace TestAssistant
{
    /// <summary>
    /// Base class for Unity Tests
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Override to implement the spawning of essential gameObjects
        /// </summary>
        /// <returns>Returns an array of the GameObjects spawned</returns>
        protected abstract GameObject[] SpawnEssentials();
        /// <summary>
        /// Destroys all of the GameObjects in the scene for clean up between tests
        /// </summary>
        protected void DestroyAllGameObjects()
        {   //Get all of the GameObjects in the scene
            try
            {
                GameObject[] objs = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
                //Loop over each and destroy them NOW
                foreach (GameObject obj in objs)
                    //Have to be explicity because of confusion between System.object and UnityEngine.Object
                    UnityEngine.Object.DestroyImmediate(obj);
            }
            catch (Exception e)
            {   //Something went wrong, probably casting the results
                Debug.LogError("Failed to destroy all GameObjects: " + e.ToString());
            }
        }
    }
}
