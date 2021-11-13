﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace QuadTree
{

    public class QuadTree<T>
    {
        /// <summary>
        /// How deep the quad tree can go
        /// </summary>
        private readonly uint _depth = 0;
        /// <summary>
        /// The relative centre of the quad tree
        /// </summary>
        private Vec2 _centre = new Vec2(0, 0);
        /// <summary>
        /// The relative scale of the QuadTree
        /// </summary>
        private Vec2 _halfExtents = new Vec2(0, 0);
        /// <summary>
        /// The sub trees going from left to right, top to bottom.
        /// </summary>
        /// <remarks>This goes in order top left, top right, bot left, bot right just like reading an english book</remarks>
        private readonly QuadTree<T>[] _subTrees = new QuadTree<T>[4];
        /// <summary>
        /// Storage location for the data
        /// </summary>
        private readonly List<Item> _data = new List<Item>();
        /// <summary>
        /// All the data on this quad tree for easy getting
        /// </summary>
        private readonly List<Item> _allData = new List<Item>();

        public QuadTree(uint maxDepth, float centreX, float centreY, float halfExtentsX, float halfExtentsY)
        {
            _depth = maxDepth;
            _centre = new Vec2(centreX, centreY);
            _halfExtents = new Vec2(halfExtentsX, halfExtentsY);
        }
        /// <summary>
        /// Adds a piece of data to the tree
        /// </summary>
        /// <param name="x">The x position of the data</param>
        /// <param name="y">The y position of the data</param>
        /// <param name="data">The data</param>
        public void Add(float x, float y, T data)
        {   //Put the data in a storage type
            Item newData = new Item(new Vec2(x, y), data);
            //Store the data globally
            _allData.Add(newData);
            //Do the math to see where to store it
            StoreItem(newData);
        }
        /// <summary>
        /// Remove specific data from the tree
        /// </summary>
        /// <param name="data">The data to remove</param>
        /// <returns>Returns true if an item was removed</returns>
        public bool Remove(T data)
        {
            int i;
            for (i = 0; i < _allData.Count; i++)
                //Check for the same item
                if (_allData[i].item.Equals(data))
                {   //Remove it from all data
                    _allData.RemoveAt(i);
                    //Search through tree to find item
                    return RemoveInternal(data);
                }
            //The item does not exist so fail
            return false;
        }
        /// <summary>
        /// Clear all data from the tree
        /// </summary>
        public void Clear()
        {   //Loop over sub trees and clear their data
            for (byte i = 0; i < 4; i++)
            {   //Null catch
                if (_subTrees[i] == null)
                    continue;
                //Tell the sub trees to clear themself
                _subTrees[i].Clear();
                _subTrees[i] = null;
            }
            //Clear stored data
            _allData.Clear();
            _data.Clear();
        }
        /// <summary>
        /// Internal function for clearing data
        /// </summary>
        /// <param name="data">The data to clear</param>
        /// <returns>Returns true if data was removed</returns>
        internal bool RemoveInternal(T data)
        {
            int i;
            for (i = 0; i < _data.Count; i++)
                //Check for the same item
                if (_data[i].item.Equals(data))
                {   //Remove the data
                    _data.RemoveAt(i);
                    return true;
                }
            //If its not on this part, tell subtrees to check
            for (i = 0; i < _subTrees.Length; i++)
                //If a subTree finds the item, return true
                //This should stop all searching for the item
                if (_subTrees[i].RemoveInternal(data))
                    return true;

            //Otherwise it could not be found
            return false;
        }
        /// <summary>
        /// Moves the position of a piece of data
        /// </summary>
        /// <param name="newX">The new x position</param>
        /// <param name="newY">The new y position</param>
        /// <param name="data">The data to move</param>
        public void MoveItem(float newX, float newY, T data)
        {   //Search for it in allData
            int i;
            Item reference = null;
            for (i = 0; i < _allData.Count; i++)
                //Check for the same item
                if (_allData[i].item.Equals(data))
                {   //If we find it, store a reference to it
                    reference = _allData[i];
                    //Remove it
                    RemoveInternal(_allData[i].item);
                }
            //If reference has not been assigned to, it failed
            if (reference == null)
                return;
            //Set the new position
            reference.relativePos.x = newX;
            reference.relativePos.y = newY;
            //Sort it back into the tree
            StoreItem(reference);
        }
        /// <summary>
        /// Gets the position of a piece of data
        /// </summary>
        /// <param name="data">The data to get the position of</param>
        /// <param name="x">Out x position</param>
        /// <param name="y">Out y position</param>
        /// <returns>Returns true if the data was found</returns>
        public bool GetPosition(T data, out float x, out float y)
        {   //Default to 0
            x = 0;
            y = 0;
            //Loop over all the data
            foreach (Item i in _allData)
                if (i.item.Equals(data))
                {   //If found, return the position of the item
                    x = i.relativePos.x;
                    y = i.relativePos.y;
                    return true;
                }
            //Fail
            return false;
        }
        /// <summary>
        /// Returns all the data in the tree
        /// </summary>
        /// <returns>Returns an array with all the items in the tree. If no items exist, returns array of 0 length</returns>
        public T[] GetAll()
        {   //Create array with all the data
            T[] allData = new T[_allData.Count];
            //Loop over the data and extract the data from the item container class
            for (int i = 0; i < allData.Length; i++)
                allData[i] = _allData[i].item;
            //Return the new array
            return allData;
        }
        /// <summary>
        /// Returns the data at the exact position
        /// </summary>
        /// <param name="x">The x position to check for</param>
        /// <param name="y">The y position to check for</param>
        /// <returns>Returns null if no data was found at the position</returns>
        public T GetData(float x, float y)
        {   //Convert data into internal vector
            Vec2 findPos = new Vec2(x, y);
            //Call the internal function. This is used because it can be used recursively
            return GetData(ref findPos);
        }
        /// <summary>
        /// Internal GetData to avoid extra memory allocation
        /// </summary>
        /// <param name="pos">A reference to the position to avoid additional memory allocation</param>
        /// <returns></returns>
        internal T GetData(ref Vec2 pos)
        {   //Search items for identical item
            foreach (Item item in _data)
                if (item.relativePos == pos)
                    //Return it if it has the same position
                    return item.item;
            //Search through the subtree
            byte tree = GetSubTree(pos);
            //Null catch
            if (_subTrees[tree] == null)
                return default;
            //Search through subTree
            T data = _subTrees[tree].GetData(ref pos);
            //Otherwise return teh default value
            return data;
        }
        /// <summary>
        /// Gets all data in a radius from a point
        /// </summary>
        /// <param name="x">The x position of the point</param>
        /// <param name="y">The y position of the point</param>
        /// <param name="radius">The radius of the circle</param>
        /// <returns>Returns an array containing all the data that was in the area</returns>
        public T[] GetData(float x, float y, float radius)
        {   //Convert to pos
            Vec2 pos = new Vec2(x, y);
            //Call internal function
            return GetData(ref pos, radius, null);
        }
        /// <summary>
        /// Internal GetData to avoid memory allocation
        /// </summary>
        /// <param name="pos">The position</param>
        /// <param name="radius">The radius</param>
        /// <param name="returnData">A list to store the return data in instead to avoid memory allocation</param>
        /// <returns>Returns null if returnData is not null. Otherwise returns an array with the data</returns>
        internal T[] GetData(ref Vec2 pos, float radius, List<T> returnData)
        {   //Get the difference in position
            float xDif = pos.x - _centre.x;
            float yDif = pos.y - _centre.y;
            //If we were given a null list, this was the first call. As such, we need to return data
            //BUT if we were given a list, put the data strait into the list
            bool returnArray = returnData == null;
            //If the circle does not clip into the AABB that makes this quad tree, return an empty array
            if (Math.Abs(yDif) > _halfExtents.y + radius || Math.Abs(xDif) > _halfExtents.x + radius)
                return returnArray ? new T[0] : null;
            //Setup storage
            if (returnData == null)
                returnData = new List<T>();
            //Now we calculate which subTrees the circle overlaps
            //Check if the origin is on the left or the circle overlaps onto the left side
            //Top left
            if (_subTrees[0] != null && xDif - radius <= 0 && yDif + radius >= 0)
                _subTrees[0].GetData(ref pos, radius, returnData);
            //Top right
            if (_subTrees[1] != null && xDif + radius > 0 && yDif + radius >= 0)
                _subTrees[1].GetData(ref pos, radius, returnData);
            //Bot left
            if (_subTrees[2] != null && xDif - radius <= 0 && yDif - radius < 0)
                _subTrees[2].GetData(ref pos, radius, returnData);
            //Bot right
            if (_subTrees[3] != null && xDif + radius > 0 && yDif - radius < 0)
                _subTrees[3].GetData(ref pos, radius, returnData);

            Vec2 dif = new Vec2(0, 0);
            //Fill the returnData with our stuff
            foreach(Item i in _data)
            {   //Get the position difference.
                //I should really create operators for this
                dif.x = pos.x - i.relativePos.x;
                dif.y = pos.y - i.relativePos.y;
                //If in circle, add to return
                if (dif.Magnitude < radius)
                    returnData.Add(i.item);
            }
            //Return the array but if we were given a list, return null to avoid excess data creation
            return returnArray ? returnData.ToArray() : null;
        }
        /// <summary>
        /// Gets all the data in the area of an AABB
        /// </summary>
        /// <param name="x">The x position of the point</param>
        /// <param name="y">The y position of the point</param>
        /// <param name="halfExtentX">The x half extents of the AABB</param>
        /// <param name="halfExtentY">The y half extents of the AABB</param>
        /// <returns>Returns an array containing all the data that was in the area</returns>
        public T[] GetData(float x, float y, float halfExtentX, float halfExtentY)
        {   //Convert to internal data types
            Vec2 pos = new Vec2(x, y);
            Vec2 extents = new Vec2(halfExtentX, halfExtentY);
            //Perform the search
            return GetData(ref pos, ref extents, null);
        }
        /// <summary>
        /// Internal GetData for AABB type get to reduce memory allocation
        /// </summary>
        /// <param name="pos">The position of the AABB</param>
        /// <param name="halfExtents">The halfExtents of the AAbB</param>
        /// <param name="returnData">List containing the return data</param>
        /// <returns></returns>
        internal T[] GetData(ref Vec2 pos, ref Vec2 halfExtents, List<T> returnData)
        {   //Get positional difference between origin
            float xDif = pos.x - _centre.x;
            float yDif = pos.y - _centre.y;
            //To return or not to return data, that is the question
            bool returnArray = returnData == null;
            //Make sure the colliders overlap
            if (Math.Abs(xDif) > _halfExtents.x + halfExtents.x || Math.Abs(yDif) > _halfExtents.y + halfExtents.y)
                return returnArray ? new T[0] : null;
            //Make sure list exists
            if (returnData == null)
                returnData = new List<T>();
            //Top Left
            if (_subTrees[0] != null && xDif - halfExtents.x <= 0 && yDif + halfExtents.y >= 0)
                _subTrees[0].GetData(ref pos, ref halfExtents, returnData);
            //Top Right
            if (_subTrees[1] != null && xDif + halfExtents.x > 0 && yDif + halfExtents.y >= 0)
                _subTrees[1].GetData(ref pos, ref halfExtents, returnData);
            //Bot Left
            if (_subTrees[2] != null && xDif - halfExtents.x <= 0 && yDif - halfExtents.y < 0)
                _subTrees[2].GetData(ref pos, ref halfExtents, returnData);
            //Bot Right
            if (_subTrees[3] != null && xDif + halfExtents.x > 0 && yDif - halfExtents.y < 0)
                _subTrees[3].GetData(ref pos, ref halfExtents, returnData);

            //Fill the returnData
            foreach(Item i in _data)
            {   //Get the difference in position to the AABB and items position
                xDif = pos.x - i.relativePos.x;
                yDif = pos.y - i.relativePos.y;
                //Check if the x difference is within the extents of the box
                if (Math.Abs(xDif) < halfExtents.x && Math.Abs(yDif) < halfExtents.y)
                    returnData.Add(i.item);
            }
            //Return the array if we didn't start with a list
            return returnArray ? returnData.ToArray() : null;
        }

        /*
         
        Add Range Variant (Unity only)
        Unity Variants for functions
         
         */
        /// <summary>
        /// For storing an item internally with reduced memory allocation
        /// </summary>
        /// <param name="data">The data to store already packaged as an item</param>
        internal void StoreItem(Item data)
        {   //Get the subTree the data should be sorted into
            byte targetTree = GetSubTree(data.relativePos);
            //If the subTree is null, then store it on this
            if (_subTrees[targetTree] == null)
                //Store the new data
                _data.Add(data);
            //If the subTree exists, store it on the subTree
            else
                _subTrees[targetTree].StoreItem(data);
            //Update the tree.
            CalculateTree();
        }
        /// <summary>
        /// Sorts the data into sub trees if they exist
        /// </summary>
        private void CalculateTree()
        {   //Make sure there is data to build the tree for
            if (_depth == 0 && _data.Count < 2)
                return;
            byte targetTree;
            //Loop over the data and put them in sub trees
            //Won't i always remain at 0? YES! So we can make this a while loop instead
            while (_data.Count > 0)
            {
                targetTree = GetSubTree(_data[0].relativePos);
                //If null, create new tree
                if (_subTrees[targetTree] == null)
                {
                    Vec2 offset = _halfExtents;
                    //If targetTree is even, left otherwise right
                    offset.x *= targetTree % 2 == 0 ? -1 : 1;
                    //If targetTree is 2 - 3, bottom otherwise top
                    offset.y *= targetTree > 1 ? -1 : 1;
                    //Create a new tree
                    _subTrees[targetTree] = new QuadTree<T>(_depth - 1, _centre.x + offset.x, _centre.y + offset.y, _halfExtents.x / 2, _halfExtents.y / 2);
                }
                //Store the data in the subTree, which will, in-turn check itself
                _subTrees[targetTree].StoreItem(_data[0]);
                //Make sure the data is removed and we don't skip something
                _data.RemoveAt(0);
            }
        }
        /// <summary>
        /// Calculates which subTree to put an item into
        /// </summary>
        /// <param name="pos">The position of the item</param>
        /// <returns>Returns the index of the subTree to put the item into</returns>
        private byte GetSubTree(Vec2 pos)
        {
            float relX, relY;
            byte targetTree;
            relX = pos.x - _centre.x;
            relY = pos.y - _centre.y;
            //Determine if we are on the left or right
            //Remember the sub trees are stored such that odd = right, even = left
            if (relX > 0)
                targetTree = 1;
            else
                targetTree = 0;
            // 0 - 1 = top, 2 - 3 = bot
            //If we are down the bottom, increase the value
            if (relY < 0)
                targetTree += 2;
            //Return the value
            return targetTree;
        }

        /// <summary>
        /// Internal vector 2 data type so that this can work outside of unity
        /// </summary>
        internal struct Vec2
        {
            public float x, y;

            public double Magnitude => Math.Sqrt((x * x) + (y * y));

            public Vec2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public static bool operator ==(Vec2 a, Vec2 b)
            {
                return a.x == b.x && a.y == b.y;
            }
            public static bool operator !=(Vec2 a, Vec2 b)
            {
                return a.x != b.x || a.y != b.y;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        /// <summary>
        /// Storage type for data
        /// </summary>
        internal class Item
        {
            public Vec2 relativePos;

            public T item;

            public Item(Vec2 pos, T item)
            {
                relativePos = pos;
                this.item = item;
            }
        }
    }
}