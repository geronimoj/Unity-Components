#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace QuadTree.Tests
{
    public class QuadTreeTests
    {
        [Test]
        public void Constructor()
        {
            QuadTree<byte> tree = new QuadTree<byte>(3, new Vector2(10, 5), new Vector2(7, 6));

            Debug.Assert(tree != null);
            Debug.Assert(tree.Depth == 3);
            Debug.Assert(tree.Center == new Vector2(10, 5));
            Debug.Assert(tree.HalfExtents == new Vector2(7, 6));

            tree = new QuadTree<byte>(5, 3, 2, 9, 10);

            Debug.Assert(tree != null);
            Debug.Assert(tree.Depth == 5);
            Debug.Assert(tree.Center == new Vector2(3, 2));
            Debug.Assert(tree.HalfExtents == new Vector2(9, 10));
        }

        [Test]
        public void Setters()
        {
            QuadTree<byte> tree = new QuadTree<byte>(0, 0, 0, 0, 0);

            Debug.Assert(tree.Depth == 0);
            Debug.Assert(tree.Center == Vector2.zero);
            Debug.Assert(tree.HalfExtents == Vector2.zero);

            uint rand0 = GetRandomUInt();

            tree.SetDepth(rand0);

            Debug.Assert(tree.Depth == rand0);
            Debug.Assert(tree.Center == Vector2.zero);
            Debug.Assert(tree.HalfExtents == Vector2.zero);

            Vector2 randPos = GetRandomVec();

            tree.SetCenter(randPos);

            Debug.Assert(tree.Depth == rand0);
            Debug.Assert(tree.Center == randPos);
            Debug.Assert(tree.HalfExtents == Vector2.zero);

            Vector2 randExtent = GetRandomVec();
            tree.SetHalfExtents(randExtent);

            Debug.Assert(tree.Depth == rand0);
            Debug.Assert(tree.Center == randPos);
            Debug.Assert(tree.HalfExtents == randExtent);
        }

        [Test]
        public void Getters()
        {
            QuadTree<byte> tree = new QuadTree<byte>(5, Vector2.zero, new Vector2(10, 10));
            
            Vector2[] positions = new Vector2[] { new Vector2(-2.4f, 3), new Vector2(-1, -1), new Vector2(56.5032f, 23.10f), new Vector2(10 ,10) };
            byte[] expectedData = new byte[] { 0, 25, 156, 233 };

            tree.Add(positions, expectedData);

            byte[] data = tree.GetAll();

            Debug.Assert(data.Length == expectedData.Length);

            for (byte i = 0; i < expectedData.Length; i++)
            {   //Test GetAll return data
                Debug.Assert(data[i] == expectedData[i]);
                //Test GetPosition
                Debug.Assert(tree.GetData(positions[i]) == expectedData[i]);
                //Test GetPosition
                if (tree.GetPosition(expectedData[i], out Vector2 pos))
                {
                    Debug.Assert(pos == positions[i]);
                }
                else//Failed
                    Debug.Assert(false);
            }

            List<byte> d = new List<byte>(tree.GetData(new Vector2(-5, 0), new Vector2(5, 5)));

            Debug.Assert(d.Count == 2);
            Debug.Assert(d.Contains(0));
            Debug.Assert(d.Contains(25));
            Debug.Assert(!d.Contains(156));
            Debug.Assert(!d.Contains(233));

            d.Clear();

            d.AddRange(tree.GetData(new Vector2(5, 5), 8));

            Debug.Assert(d.Count == 2);
            Debug.Assert(d.Contains(0));
            Debug.Assert(!d.Contains(25));
            Debug.Assert(!d.Contains(156));
            Debug.Assert(d.Contains(233));
        }

        [Test]
        public void Add()
        {
            QuadTree<byte> tree = new QuadTree<byte>(2, Vector2.zero, new Vector2(10, 10));

            tree.Add(4, 2, 7);

            Debug.Assert(tree.GetData(4, 2) == 7);

            tree.Add(new Vector2(10, 8), 100);

            Debug.Assert(tree.GetData(10, 8) == 100);

            tree.Add(new Vector2[] { Vector2.zero, new Vector2(9, 3) }, new byte[] { 2, 6 });

            byte[] expectedData = new byte[] { 7, 100, 2, 6 };
            byte[] data = tree.GetAll();

            for (byte i = 0; i < 4; i++)
                Debug.Assert(data[i] == expectedData[i]);
        }

        [Test]
        public void Remove()
        {
            QuadTree<byte> tree = new QuadTree<byte>(5, Vector2.zero, new Vector2(10, 10));

            tree.Add(new Vector2(3, 7), 8);
            tree.Add(new Vector2(-3.423f, 73), 205);
            tree.Add(new Vector2(1302, 53e5f), 125);

            byte[] data = tree.GetAll();

            Debug.Assert(data.Length == 3);

            tree.Remove(125);

            data = tree.GetAll();
            Debug.Assert(data.Length == 2);
            Debug.Assert(data[0] == 8);
            Debug.Assert(data[1] == 205);
        }

        [Test]
        public void Clear()
        {
            QuadTree<byte> tree = new QuadTree<byte>(3, 0, 0, 10, 10);
            //Fill it with random data
            for (byte i = 0; i < byte.MaxValue; i++)
                tree.Add(GetRandomVec(-10, 10), (byte)Random.Range(0, 255));

            byte[] data = tree.GetAll();

            Debug.Assert(data.Length == byte.MaxValue);

            tree.Clear();

            data = tree.GetAll();

            Debug.Assert(data.Length == 0);
        }

        private uint GetRandomUInt()
        {
            return (uint)Random.Range(0, 100);
        }

        private Vector2 GetRandomVec()
        {
            return new Vector2(Random.Range(float.MinValue, float.MaxValue), Random.Range(float.MinValue, float.MaxValue));
        }
        private Vector2 GetRandomVec(float min, float max)
        {
            return new Vector2(Random.Range(min, max), Random.Range(min, max));
        }
    }
}
#endif