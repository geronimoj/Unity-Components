//Create by Luke Jones - A long time ago

using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace Factories
{
    /// <summary>
    /// Base class for factories for storing data
    /// </summary>
    /// <typeparam name="T">The type of factory this is</typeparam>
    public abstract class Factory<T> : FactoryBase
    {
        /// <summary>
        /// List of the selectable T
        /// </summary>
        [SerializeField]
        private List<T> _data = new List<T>();
        /// <summary>
        /// The value that represents null/nothing. Some data types cannot be nulled or new-ed
        /// </summary>
        [Tooltip("The return value that represents null/nothing")]
        [SerializeField]
        private T _nullValue = default;
        /// <summary>
        /// The default value
        /// </summary>
        public T NullValue => _nullValue;
        /// <summary>
        /// The number of T stored
        /// </summary>
        public int Length => _data.Count;
        /// <summary>
        /// Gets a T via index
        /// </summary>
        /// <param name="index">The index of the mode</param>
        /// <returns>The T to return. Returns null if the index is invalid</returns>
        public T this[int index]
        {
            get
            {   //Make sure index is valid
                if (index < 0 || index >= _data.Count)
                    return _nullValue;
                return _data[index];
            }
        }
        /// <summary>
        /// Gets the index of T
        /// </summary>
        /// <param name="m">The T to get the index of</param>
        /// <returns>Returns the index of the T. Returns -1 if it could not be found</returns>
        public int this[T m]
        {
            get
            {   //Loop over the T
                for (int i = 0; i < Length; i++)
                    //If the T is the given T return the index of it
                    if (_data[i].Equals(m))
                        return i;
                //Return an invalid index
                return -1;
            }
        }
        /// <summary>
        /// Readonly version of the T.
        /// </summary>
        public ReadOnlyCollection<T> Data => _data.AsReadOnly();
    }
    /// <summary>
    /// Ultimate base class for all factories. This is something for the FactoryInitializer to do
    /// </summary>
    public abstract class FactoryBase : ScriptableObject
    {
        /// <summary>
        /// Initilizes the factory
        /// </summary>
        public abstract void Initialize();
    }
}