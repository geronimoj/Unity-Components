using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Helpers
{
    public static class TypeHelper
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
            catch (Exception e)
            { Debug.LogError("Failed to cast " + obj.GetType() + " to " + className + ": " + e.ToString()); return; }
            //Set the base class to be the newly created class
            baseClass = type;
        }
        /// <summary>
        /// Returns all the fields on a given type and its base type and so on reguardless as to wether its public or nonpublic
        /// </summary>
        /// <param name="type">The type to get the fields of</param>
        /// <returns>The fields as an array. Returns empty array if there are no fields</returns>
        public static IEnumerable<FieldInfo> GetAllFields(Type type)
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
            return fields;
        }
        /// <summary>
        /// Gets all the fields on a type of the type T
        /// </summary>
        /// <typeparam name="T">The type of field to look for</typeparam>
        /// <param name="type">object to look for them on</param>
        /// <returns>Returns an array for field info. Will be size 0 if no fields are found</returns>
        public static IEnumerable<FieldInfo> GetFieldsOfType<T>(Type type)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            //Get the fields, both public and non public on this instace
            FieldInfo[] typeFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //For each field, add it but only if its the same type as T
            foreach (FieldInfo f in typeFields)
                if (f.FieldType == typeof(T))
                    fields.Add(f);
            //Return as enumerable to save array allocation. If they really want an array, they can make one themself.
            return fields;
        }
        /// <summary>
        /// Get all fields that can be serialized on the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetSerializableFields(Type type)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            FieldInfo[] typeFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Type serializeType = typeof(SerializeField);

            foreach (FieldInfo field in typeFields)
            {   // Public
                if (field.IsPublic)
                {   // Is serialized
                    if (!field.Attributes.HasFlag(FieldAttributes.NotSerialized))
                    fields.Add(field);
                    continue;
                }
                // Private or protected
                foreach (var attribute in field.CustomAttributes)
                    if (attribute.AttributeType == serializeType)
                    {   // Is serialized
                        fields.Add(field);
                        break;
                    }
            }
            // Return as enumerable to avoid additional memory allocation.
            return fields;
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
        /// <summary>
        /// Copies the fields (fields) of an object into another object. Only fields of identical name & type are copied.
        /// </summary>
        /// <param name="from">The object to copy from</param>
        /// <param name="to">The object to copy to</param>
        /// <param name="flags">The flags used to define which fields to move</param>
        /// <returns>Returns paramameter "to". If "to" is a class, this can be ignored. If a struct, pass by values rules require this</returns>
        /// <remarks>This does not care about the Types of "from" and "to", it matches fields by Type & name. This WILL NOT create a new "to" object. 
        /// You need to ensure that "to" is already allocated, otherwise use templated variant</remarks>
        public static object CopyFields(in object from, object to, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {   //Cannot copy null
            if (from == null || to == null)
                throw new NullReferenceException("Cannot copy " + ((from == null) ? "Null" : "'From'")
                    + " into " + ((to == null) ? "Null" : "'To'"));

            Type fType = from.GetType();
            Type tType = to.GetType();
            //Gather all fields
            var fromFields = fType.GetFields(flags);
            var toFields = tType.GetFields(flags);
            //Copy fields of matching type & name
            foreach (FieldInfo field in fromFields)
                foreach (FieldInfo destinationField in toFields)
                {   //Incorrect type
                    if (field.FieldType != destinationField.FieldType
                        //Name incorrect
                        || field.Name != destinationField.Name)
                        continue;
                    //Copy the field over
                    destinationField.SetValue(to, field.GetValue(from));
                }
            // Return "to". This is necessary if copying to a struct, we need to return the struct since, if we use "out object to", we don't get to know "to"'s type
            // Now, we could make this a templated function but its simpler flow for most cases
            return to;
        }
        /// <summary>
        /// Copies the fields (fields) of an object into another object. Only fields of identical name & type are copied. This does not care about type
        /// </summary>
        /// <typeparam name="TOutType">The type that "to" should be</typeparam>
        /// <param name="from">The object to copy fields from</param>
        /// <param name="to">The object fields were coppied into</param>
        /// <param name="flags">The flags used to define which fields to move</param>
        /// <remarks>This does not care about the Types of "from" and "to", it matches fields by Type & name. 
        /// This WILL create a new "to" object, use the non-templated type if you do not want to allocate a new object</remarks>
        public static void CopyFields<TOutType>(in object from, out TOutType to, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            if (from == null)
                throw new NullReferenceException("Cannot copy from 'null' object");
            //Create instance
            to = Activator.CreateInstance<TOutType>();
            //Incase TToType is struct, we need to assign to after function. If TToType is a class, this does nothing.
            //to = (TOutType)Copyfields(in from, to, flags);
            //----- I don't do this anymore (despite making logic in one place) because it removes a pass by value for struct types. If TOutType contains a lot of data
            //      & is a struct, this significantly improves performance by not having to copy the return type again

            Type fType = from.GetType();
            //Gather fields
            var fromFields = fType.GetFields(flags);
            var toFields = typeof(TOutType).GetFields(flags);
            //Copy fields of matching type & name
            foreach (FieldInfo field in fromFields)
                foreach (FieldInfo destinationField in toFields)
                {   //Incorrect type
                    if (field.FieldType != destinationField.FieldType
                        //Name incorrect
                        || field.Name != destinationField.Name)
                        continue;
                    //Copy the field over
                    destinationField.SetValue(to, field.GetValue(from));
                }
        }
        /// <summary>
        /// Copies all fields from one object to another.
        /// </summary>
        /// <typeparam name="TOutType">The type of object</typeparam>
        /// <param name="from">The object to copy from</param>
        /// <param name="to">The object with the copied values</param>
        /// <param name="flags">The flags used to define which fields to move</param>
        /// <remarks>This variant will create a new instance of "to", use the non-'out' variant if you have already created an instance</remarks>
        public static void CopyFields<TOutType>(in TOutType from, out TOutType to, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            if (from == null)
                throw new NullReferenceException("Cannot copy from 'null' object");
            //Create instance
            to = Activator.CreateInstance<TOutType>();
            //Gather fields
            var fields = typeof(TOutType).GetFields(flags);
            //Copy fields of matching type & name
            foreach (FieldInfo field in fields)
                //Copy the field over
                field.SetValue(to, field.GetValue(from));
        }
        /// <summary>
        /// Copies all fields from one object to another.
        /// </summary>
        /// <typeparam name="TOutType">The type of object</typeparam>
        /// <param name="from">The object to copy from</param>
        /// <param name="to">The object with the copied values</param>
        /// <param name="flags">The flags used to define which fields to move</param>
        /// <remarks>This variant requires "to" to already be initialized</remarks>
        public static TOutType CopyFields<TOutType>(in TOutType from, TOutType to, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {   //Cannot copy null
            if (from == null || to == null)
                throw new NullReferenceException("Cannot copy " + ((from == null) ? "Null" : "'From'")
                    + " into " + ((to == null) ? "Null" : "'To'"));
            //Gather fields
            var fields = typeof(TOutType).GetFields(flags);
            //Copy fields of matching type & name
            foreach (FieldInfo field in fields)
                //Copy the field over
                field.SetValue(to, field.GetValue(from));

            return to;
        }

        public static object InvokeMethod(in object @object, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, params object[] parameters)
        {   //Object is null
            if (@object == null)
                throw new NullReferenceException("Object is null");

            Type type = @object.GetType();
            MethodInfo method = type.GetMethod(methodName, flags);
            //Method not found, error
            if (method == null)
                throw new Exception("Method not found");

            return method.Invoke(@object, parameters);
        }
    }
}