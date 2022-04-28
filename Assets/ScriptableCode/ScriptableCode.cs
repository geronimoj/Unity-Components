﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeObjects
{
    /// <summary>
    /// Generic scriptableObject class for storing code
    /// </summary>
    public abstract class ScriptableCode : ScriptableObject
    {
        /// <summary>
        /// Called to run the code
        /// </summary>
        /// <param name="parameters">The parameters of the function</param>
        public abstract void Run(params object[] parameters);
        /// <summary>
        /// Convinience function for running a collection of scriptable codes. Deals with nulls.
        /// </summary>
        /// <param name="code">The scriptable codes to run</param>
        /// <param name="parameters">The parameters to run on all scriptableCodes</param>
        public static void Invoke(IEnumerable<ScriptableCode> code, params object[] parameters)
        {   //Avoid null
            if (code == null)
                return;

            foreach (var c in code)
                SafeRun(c, parameters);
        }
        /// <summary>
        /// Runs the code and catches any exceptions, logging them to the console.
        /// </summary>
        /// <param name="code">The code to run</param>
        /// <param name="parameters">The parameters for the scriptableCode</param>
        public static void SafeRun(ScriptableCode code, params object[] parameters)
        {   //Null catch
            if (!code)
                return;
            //Run in try catch
            try
            {
                code.Run(parameters);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Scriptable Code '" + code.name + "' execute failed: " + e.ToString());
            }
        }
    }
    /// <summary>
    /// Generic scriptableObject class for storing code with return type
    /// </summary>
    /// <typeparam name="T">The return type of Run()</typeparam>
    public abstract class ScriptableCode<T> : ScriptableObject
    {
        /// <summary>
        /// Called to run the code
        /// </summary>
        /// <param name="parameters">The parameters of the function</param>
        /// <returns>Presumably something, otherwise use the non-templated version</returns>
        public abstract T Run(params object[] parameters);
        /// <summary>
        /// Convinience function for running a collection of scriptable codes. Deals with nulls.
        /// </summary>
        /// <param name="code">The scriptable codes to run</param>
        /// <param name="parameters">The parameters to run on all scriptableCodes</param>
        /// <returns>Returns a list with the returns from each scriptable code.</returns>
        public static List<T> Invoke(IEnumerable<ScriptableCode<T>> code, params object[] parameters)
        {
            List<T> rets = new List<T>();
            //Avoid null
            if (code == null)
                //Always return something to make it easier on other systems.
                //Don't have to account for null.
                return rets;

            foreach (var c in code)
                rets.Add(SafeRun(c, parameters));

            return rets;
        }
        /// <summary>
        /// Runs the code and catches any exceptions, logging them to the console.
        /// </summary>
        /// <param name="code">The code to run</param>
        /// <param name="parameters">The parameters for the scriptableCode</param>
        /// <returns>Returns default if the code fails or code is null</returns>
        public static T SafeRun(ScriptableCode<T> code, params object[] parameters)
        {   //Null catch
            if (!code)
                return default;
            //Run in try catch
            try
            {
                return code.Run(parameters);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Scriptable Code '" + code.name + "' execute failed: " + e.ToString());
                return default;
            }
        }
    }
}