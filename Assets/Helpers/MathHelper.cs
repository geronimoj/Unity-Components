using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public static class MathHelper
    {
        #region Unity
        public static bool GetOverlappingPoint(Vector2 originA, Vector2 dirA, Vector2 originB, Vector2 dirB, out Vector2 overlapPoint)
        {
            dirA.Normalize();
            dirB.Normalize();
            float a1, a2;
            a1 = Mathf.Atan2(dirA.y, dirA.x);
            a2 = Mathf.Atan2(dirB.y, dirB.x);

            if (a1 >= Math.PI)
                a1 -= (float)Math.PI;
            if (a2 >= Math.PI)
                a2 -= (float)Math.PI;

            a1 -= a2;
            a1 = Math.Abs(a1);
            //Make sure lines are not parallel
            if (a1 < 0.0001f)
            {
                overlapPoint = Vector2.zero;
                return false;
            }


            overlapPoint = Vector2.zero;
            //Calculate the difference between PI / 2 and the angle difference between the two points
            a2 = (float)(Math.PI / 2);
            a1 -= a2;
            //Check lines are not perpendicular
            if (a1 <  0.0001f)
            {   //Are perpendicular
                Vector2 aToB = originB - originA;
                //Use Dot to get distance
                a1 = Vector2.Dot(aToB, dirA);
                overlapPoint = originA + dirA * a1;
                return true;
            }

            //Check lines are not 1,0 or 0,1
            if ((dirA.y == 0 && dirB.x == 0))
            {   //A is horizontal, B is vertical
                overlapPoint = new Vector2(originB.x, originA.y);
                return true;
            }
            if ((dirA.x == 0 && dirB.y == 0))
            {   //A is vertical, B is horizontal
                overlapPoint = new Vector2(originA.x, originB.y);
                return true;
            }

            float c1, c2 , m1, m2;
            //Rise over run
            m1 = dirA.y / dirA.x;
            m2 = dirB.y / dirB.x;

            c1 = originA.y - (originA.x * m1);
            c2 = originB.y - (originB.x * m2);

            //mx - mx = c - c
            //mx - mx
            m1 -= m2;
            //c - c
            float c = c2 - c1;

            overlapPoint = new Vector2(c / m1, (m2 * originB.x) + c2);
            return true;
        }
        #endregion
    }
}