using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Helpers
{
    public class TypeHelper
    {
        /// <summary>
        /// Takes the full name of a class as a string and creates an instance of it, storing it as a base class
        /// </summary>
        /// <typeparam name="T">The class to create</typeparam>
        /// <param name="className">The full name of the class</param>
        /// <param name="baseClass">The base class to store it in</param>
        public static void StringToClass<T>(string className, ref T baseClass)
        {   //Attempt to convert the string name into a type.
            Type t;
            try
            {
                t = Type.GetType(className);
            }
            catch (Exception e) { Debug.LogError("Failed to get class type from string: " + e.ToString()); return; }
            //Attempt to create an instance of the type
            T type;
            object obj;
            try
            {
                obj = System.Activator.CreateInstance(t);
            }
            catch (Exception e)
            { Debug.LogError("Failed to create instance of class: " + e.ToString()); return; }

            try
            {
                type = (T)obj;
            }
            catch(Exception e)
            { Debug.LogError("Failed to cast " + obj.GetType() + " to " + className + ": " + e.ToString()); return; }
            //Set the base class to be the newly created class
            baseClass = type;
        }
        /// <summary>
        /// Returns all the fields on a given type and its base type and so on reguardless as to wether its public or nonpublic
        /// </summary>
        /// <param name="type">The type to get the fields of</param>
        /// <returns>The fields as an array. Returns empty array if there are no fields</returns>
        public static FieldInfo[] GetAllFields(Type type)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            //While type is not null
            while (type != null)
            {   //Get all public and nonpublic fields
                FieldInfo[] typeFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                //Store them in fields
                foreach (FieldInfo f in typeFields)
                    fields.Add(f);
                //Now point to the base type
                //This should eventually become null
                type = type.BaseType;
            }
            //Return the fields found
            return fields.ToArray();
        }
        /// <summary>
        /// Gets all the fields on a type of the type T
        /// </summary>
        /// <typeparam name="T">The type of field to look for</typeparam>
        /// <param name="type">object to look for them on</param>
        /// <returns>Returns an array for field info. Will be size 0 if no fields are found</returns>
        public static FieldInfo[] GetFieldsOfType<T>(Type type)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            //Get the fields, both public and non public on this instace
            FieldInfo[] typeFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //For each field, add it but only if its the same type as T
            foreach (FieldInfo f in typeFields)
                if (f.FieldType == typeof(T))
                    fields.Add(f);
            //Return the array
            return fields.ToArray();
        }
        /// <summary>
        /// Gets a field by name
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The object to get the field on</param>
        /// <returns>Returns null if type is null. Otherwise returns the field info.</returns>
        public static FieldInfo GetFieldByName(string name, Type type)
        {   //Make sure not null
            if (type == null)
                return null;
            //Attempt to get the field
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //Return the field
            return field;
        }
    }
}