using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BufferGetComponents
{
    const int DEFAULT_CAPACITY = 100;
    /// <summary>
    /// Set get buffer capacity to  1, in the majority of cases, its probably only going to get 1 or 2 item per object
    /// </summary>
    const int GET_BUFFER_CAPACITY = 1;

    /// <summary>
    /// Buffer to write to for GetComponentsInChildren.
    /// </summary>
    static List<Component> getBuffer = null;

    /// <summary>
    /// Gets a temporary list of components on a single buffer, shared throughout the project
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <param name="minCapacity">The minimum capcity of the buffer</param>
    /// <returns></returns>
    public static BufferedResults<T> GetComponents<T>(this Component component, int minCapacity = DEFAULT_CAPACITY) where T : Component
    {
        // The buffer system used an object type List class, so we cannot directly had it into GetComponents
        // Thus we use the getBuffer.
        var buffer = BufferList<Component>.Get(minCapacity);
        getBuffer ??= new List<Component>(GET_BUFFER_CAPACITY);

        component.GetComponents(typeof(T), getBuffer);
        buffer.AddRange(getBuffer);

        return new BufferedResults<T>(buffer);
    }

    /// <summary>
    /// Gets a temporary list of components on a single buffer, shared throughout the project
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <param name="includeInactive">Whether to include inactive child gameObjects in search</param>
    /// <param name="minCapacity">The minimum capcity of the buffer</param>
    /// <returns></returns>
    public static BufferedResults<T> GetComponentsInChildren<T>(this Component component, bool includeInactive, int minCapacity = DEFAULT_CAPACITY) where T : Component
    {
        /*
         * I'm sorry, why the fuck is there no generic type buffer writting GetComponentsInChildren? There is only explicitly types variants
         * This defeats the entire purpose of this buffer system which is trying to mitigate allocation caused by these systems.
         * 
         * We cannot even manually re-create the function because GetComponents won't start at an offset. List.GetRange appears to allocate a new list
         * so that's off the table as well...
         * 
         * The searching isn't the hard part, its the fucking buffer. We'd need at least 2. 1 to handle the return object, another to store the temporary list of components.
         */
        var buffer = BufferList<Component>.Get(minCapacity);
        getBuffer ??= new List<Component>(GET_BUFFER_CAPACITY);

        // Begin manually iterating over the children to generate our component output
        GetComponentsAndRecursivelySearch(component.transform);

        // Return the buffered results
        return new BufferedResults<T>(buffer);

        void GetComponentsAndRecursivelySearch(Transform target)
        {
            // Get the components and add them to the return buffer
            target.GetComponents(typeof(T), getBuffer);
            buffer.AddRange(getBuffer);

            // Go over each child and search them for components as well
            int childCount = target.childCount;
            for(int i = 0; i < childCount; i++)
            {
                var child = target.GetChild(i);
                var childObj = child.gameObject;

                // Don't include inactive objects in the search
                if (!includeInactive && !childObj.activeInHierarchy)
                    continue;

                GetComponentsAndRecursivelySearch(child);
            }
        }
    }
    /// <summary>
    /// Gets a temporary list of components on a single buffer, shared throughout the project
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <param name="includeInactive">Whether to include inactive parent gameObjects in search</param>
    /// <param name="minCapacity">The minimum capcity of the buffer</param>
    /// <returns></returns>
    public static BufferedResults<T> GetComponentsInParent<T>(this Component component, bool includeInactive, int minCapacity = DEFAULT_CAPACITY) where T : Component
    {
        /*
         * I'm sorry, why the fuck is there no generic type buffer writting GetComponentsInParent? There is only explicitly types variants
         * This defeats the entire purpose of this buffer system which is trying to mitigate allocation caused by these systems.
         * 
         * We cannot even manually re-create the function because GetComponents won't start at an offset. List.GetRange appears to allocate a new list
         * so that's off the table as well...
         * 
         * The searching isn't the hard part, its the fucking buffer. We'd need at least 2. 1 to handle the return object, another to store the temporary list of components.
         */
        var buffer = BufferList<Component>.Get(minCapacity);
        getBuffer ??= new List<Component>(GET_BUFFER_CAPACITY);

        // Begin manually iterating over the children to generate our component output
        GetComponentsAndRecursivelySearch(component.transform);

        // Return the buffered results
        return new BufferedResults<T>(buffer);

        void GetComponentsAndRecursivelySearch(Transform target)
        {
            // Get the components and add them to the return buffer
            target.GetComponents(typeof(T), getBuffer);
            buffer.AddRange(getBuffer);

            // Check each parent
            Transform parent = target.parent;

            if (parent != null)
            {   
                // I think this is how it works. Stops recursively searching
                if (includeInactive == false &&
                    parent.gameObject.activeInHierarchy == false)
                    return;

                // Continue checks on the parent
                GetComponentsAndRecursivelySearch(parent);
            }
        }
    }

    /// <summary>
    /// Deletes all allocated buffers, sending them to GC
    /// </summary>
    public static void Dispose()
    {
        getBuffer = null;
    }

    public struct BufferedResults<T> : IEnumerable<T>, IEnumerable where T : Component
    {
        /// <summary>
        /// The buffer we are reading from
        /// </summary>
        BufferList<Component> buffer;

        public readonly T this[int index]
        {
            get
            {
                // Throw relevant exceptions when directly indexing
                if (index >= buffer.Count)
                    throw new ArgumentOutOfRangeException();

                return buffer[index] as T;
            }
        }

        public BufferedResults(BufferList<Component> buffer)
        {
            this.buffer = buffer;
        }

        public readonly IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(buffer);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The buffer we are reading from
            /// </summary>
            BufferList<Component> buffer;
            /// <summary>
            /// The index the enumerator is currently at
            /// </summary>
            int index;

            /// <summary>
            /// The current object the enumeartor is targeting
            /// </summary>
            public T Current
            {
                get
                {   // If invalid buffer or buffer is out of range
                    if (index >= buffer.Count)
                        return default;

                    return buffer[index] as T;
                }
            }

            object IEnumerator.Current => Current;

            public Enumerator(BufferList<Component> buffer)
            {
                this.buffer = buffer;
                index = 0;
            }

            public void Dispose()
            {
                buffer = default;
                index = 0;
            }

            public bool MoveNext()
            {   
                index++;
                return index < buffer.Count;
            }

            public void Reset()
            {
                index = 0;
            }
        }
    }
}
