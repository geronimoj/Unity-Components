using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{
    /// <summary>
    /// Stores the players capsual collider information with related access and manipulation functions
    /// </summary>
    [System.Serializable]
    public sealed class CapsualCollider : ColliderInfo
    {
        /// <summary>
        /// The collider for this collider :P
        /// </summary>
        private CapsuleCollider collider;
        /// <summary>
        /// The radius of the capsual
        /// </summary>
        [SerializeField]
        [Tooltip("The radius of the capsual")]
        private float radius;
        /// <summary>
        /// How far below the origin the capsual extends. Includes radius
        /// </summary>
        [SerializeField]
        [Tooltip("How far below the origin the capsual extends. Includes radius.")]
        private float lowerHeight;
        /// <summary>
        /// How far above the origin the capsual extends. Includes radius.
        /// </summary>
        [SerializeField]
        [Tooltip("How far above the origin the capsual extends. Includes radius")]
        private float upperHeight;
        /// <summary>
        /// Determines wether the position offset of the collider should be read as local or global co-ordinates
        /// TLDR: Should positionOffset be affected by origin transform rotation.
        /// </summary>
        [Tooltip("Should Position Offset of the collider be affected by origin transform rotation.")]
        [SerializeField]
        private bool positionOffsetIsGlobal;
        /// <summary>
        /// Determines if the rotation of the collider is in local or global co-ordinates
        /// </summary>
        [Tooltip("Should orienation be in local or global co-ordinates")]
        [SerializeField]
        private bool rotationIsGlobal;
        /// <summary>
        /// The positional offset from the origin of the collider
        /// </summary>
        private Vector3 posOffset;
        /// <summary>
        /// The orientation of the collider. Orientation points towards the top of the capsual
        /// </summary>
        [SerializeField]
        private Vector3 orientation;
        /// <summary>
        /// Sets reference to the transform to the given transform.
        /// </summary>
        /// <param name="t">The transform to be set</param>
        public override void SetTransform(Transform t)
        {
            base.SetTransform(t);
            //Spit out an error if the transform is invalid
            if (CheckValidTransform())
            {
                GameObject col = new GameObject("Collider", new System.Type[] { typeof(CapsuleCollider) });
                //Get collider and set parent
                collider = col.GetComponent<CapsuleCollider>();
                col.transform.parent = origin;
                //Update the collider
                UpdateCollider();
            }
        }
        /// <summary>
        /// Gets or Sets the Orientation. If Orientation is zero, -Gravity will be returned instead
        /// </summary>
        public Vector3 Orientation
        {
            get
            {   //If orientation is 0, return -gravity
                if (orientation == Vector3.zero)
                    return -GravityDirection;
                //If the rotation is global, return the rotation
                if (rotationIsGlobal)
                    return orientation;

                //Return orientation based on origins rotation
                return (origin.forward * orientation.z
                    + origin.right * orientation.x
                    + origin.up * orientation.y).normalized;
            }
            set
            {
                orientation = value.normalized;
                //Update the collider. This is necessary since, if lowerHeight + upperHeight are not the same, we are going to have a new position
                UpdateCollider();
            }
        }
        /// <summary>
        /// Gets or Sets the positional offset from the transform of the collider
        /// </summary>
        public Vector3 PositionOffset
        {
            get
            {
                return posOffset;
            }
            set
            {
                posOffset = value;
                //Self explanitory
                UpdateCollider();
            }
        }

        #region GetPoints
        /// <summary>
        /// Returns the position of the sphere that represents the lower part of the capsual
        /// </summary>
        /// <returns>Returns the position of the sphere that represents the lower part of the capsual</returns>
        public Vector3 GetLowerPoint()
        {   //Call get LowestPoint but with and offset of -Radius
            return GetLowestPoint(-Radius);
        }
        /// <summary>
        /// Returns the position of the sphere that represents the lower part of the capsual.
        /// The offset is added onto lowerHeight before calculating the point
        /// </summary>
        /// <param name="offset">An offset applied to lowerHeight before the point is calculated</param>
        /// <returns>Returns the position of the sphere that represents the lower part of the capsual</returns>
        public Vector3 GetLowerPoint(float offset)
        {   //Call Get Lowest Point with an offset of offset & -Radius
            return GetLowestPoint(-Radius + offset);
        }
        /// <summary>
        /// Returns the position at the bottom of the capsual
        /// </summary>
        /// <returns>The point at the bottom most point of the capsual</returns>
        public override Vector3 GetLowestPoint()
        {   //Call GetLowestPoint with an offset of 0
            return GetLowestPoint(0);
        }
        /// <summary>
        /// Returns the point at the bottom of the capsual
        /// </summary>
        /// <param name="offset">An offset applied to lowerHeight before the point is calculated</param>
        /// <returns>The bottom most point of the capsual</returns>
        public Vector3 GetLowestPoint(float offset)
        {   //Make sure the transform is valid, otherwise return 0
            if (CheckValidTransform())
                return Vector3.zero;
            //Calculate the location of the lower circle
            return GetOriginPosition() - Orientation * (LowerHeight + offset);
        }
        /// <summary>
        /// Returns the point of the sphere that reprsents the top of the capsual
        /// </summary>
        /// <returns>The point the sphere that makes up the top of the capsual exists at</returns>
        public Vector3 GetUpperPoint()
        {   //Keep all the math in one place
            return GetHighestPoint(-Radius);
        }
        /// <summary>
        /// Returns the point of the sphere that reprsents the top of the capsual
        /// Offset is added to upperHeight before calculating the point
        /// </summary>
        /// <param name="offset">An offset applied to upperHeight before calculating the point</param>
        /// <returns>The point the sphere that makes up the top of the capsual exists at</returns>
        public Vector3 GetUpperPoint(float offset)
        {   //Keep all the math in one place
            return GetHighestPoint(-Radius + offset);
        }
        /// <summary>
        /// Returns the point at the top of the capsual
        /// </summary>
        /// <returns>The upper most point of the capsual</returns>
        public Vector3 GetHighestPoint()
        {   //Keep all the math in one place
            return GetHighestPoint(0);
        }
        /// <summary>
        /// Returns the point at the top of the capsual
        /// Offset is added to UpperPoint before calculation
        /// </summary>
        /// <param name="offset">The offset to upperPoint</param>
        /// <returns>The upper most point of the capsual with an offset</returns>
        public Vector3 GetHighestPoint(float offset)
        {   //Make sure the transform is valid
            if (CheckValidTransform())
                return Vector3.zero;
            //Calculate the point
            return GetOriginPosition() + Orientation * (UpperHeight + offset);
        }
        #endregion
        /// <summary>
        /// A Get Set for the radius of this capsual
        /// </summary>
        public float Radius
        {
            get
            {   //If radius is 0. Report an error
                if (radius == 0)
                    Debug.LogError("Radius is 0");
                return radius;
            }
            set
            {   //Make sure the value is a positive or 0 number
                if (value < 0)
                    value = 0;
                radius = value;
                //
                UpdateCollider();
            }
        }
        /// <summary>
        /// Returns the true radius of the player. The Radius + the collision Offset
        /// </summary>
        public float TrueRadius
        {
            get
            {
                return Radius + CollisionOffset;
            }
        }
        /// <summary>
        /// A Get Set for the Upper Height of the capsual.
        /// Upper Height is the distance above the origin the capsual extends
        /// </summary>
        public float UpperHeight
        {
            get
            {
                return upperHeight;
            }
            set
            {
                upperHeight = value;
                UpdateCollider();
            }
        }
        /// <summary>
        /// A Get Set for the Lower Height of the capsual.
        /// Lower Height is the distance below the origin the capsual extends
        /// </summary>
        public float LowerHeight
        {
            get
            {
                return lowerHeight;
            }
            set
            {
                lowerHeight = value;
                UpdateCollider();
            }
        }
        /// <summary>
        /// Returns the total height of the capsual
        /// </summary>
        public float Height
        {
            get
            {
                return lowerHeight + upperHeight;
            }
        }
        /// <summary>
        /// Toggle whether position offset is global
        /// </summary>
        public bool PositionOffsetIsGlobal
        {
            get { return positionOffsetIsGlobal; }
            set
            {
                positionOffsetIsGlobal = value;
                UpdateCollider();
            }
        }
        /// <summary>
        /// Toggle whether orientation is global
        /// </summary>
        public bool OrientationIsGlobal
        {
            get { return rotationIsGlobal; }
            set
            {
                rotationIsGlobal = value;
                UpdateCollider();
            }
        }
        /// <summary>
        /// Gets the origin of this collider.
        /// Gets origins position offset by positionOffset
        /// </summary>
        /// <returns>Returns the location of the capsual collider</returns>
        public Vector3 GetOriginPosition()
        {   //Make sure the transform is valid
            if (CheckValidTransform())
                return Vector3.zero;
            if (!positionOffsetIsGlobal)
                return origin.position + origin.right * posOffset.x + origin.up * posOffset.y + origin.forward * posOffset.z;
            else
                return origin.position + posOffset;
        }
        /// <summary>
        /// Determines the distance to the edge of the capsual from a direction dir
        /// </summary>
        /// <param name="dir">The direction to get the distance of</param>
        /// <returns>Returns the distance to the edge</returns>
        public float DistanceToEdge(Vector3 dir)
        {
            return DistanceToEdgeGivenRadius(dir, Radius);
        }
        /// <summary>
        /// Determines the distance to the edge of the capsual from a direction dir with collisionOffset
        /// </summary>
        /// <param name="dir">The direction to get the distance of</param>
        /// <returns>Returns the distance to the edge</returns>
        public float DistanceToEdgeWithOffset(Vector3 dir)
        {
            return DistanceToEdgeGivenRadius(dir, Radius + CollisionOffset);
        }
        /// <summary>
        /// Calculates the distance to the edge from a direction
        /// </summary>
        /// <param name="dir">The direction of the check</param>
        /// <param name="radius">The radius of the capsual</param>
        /// <returns>Returns the distance to the edge</returns>
        private float DistanceToEdgeGivenRadius(Vector3 dir, float radius)
        {
            dir.Normalize();
            float dot = Vector3.Dot(dir, Orientation);

            //If dot is > 0, then we need to check against the upper point.
            if (dot > 0)
                return DistanceToEdgeMath(dir, radius, GetUpperPoint());
            //If dot is < 0, then we need to check against the lower point, instead of upper point.
            else if (dot < 0)
                return DistanceToEdgeMath(dir, radius, GetLowerPoint());
            //Return the radius
            return radius;
        }
        /// <summary>
        /// The maths used by DistanceToEdgeGivenRadius
        /// </summary>
        /// <param name="dir">The direction of the line</param>
        /// <param name="radius">The radius of the capsual</param>
        /// <param name="circlePoint">The point the circle occupies</param>
        /// <returns>Returns the distance to the edge</returns>
        private float DistanceToEdgeMath(Vector3 dir, float radius, Vector3 circlePoint)
        {
            Vector3 sPoint = GetOriginPosition();
            //Get an orientation vector
            Vector3 ori = circlePoint - sPoint;

            //First we need to figure out if we are exiting in the middle bit or the circle ends.
            Vector3 cross = Vector3.Cross(ori.normalized, dir);
            cross = (circlePoint + (cross.normalized * radius));
            cross -= sPoint;

            float dot = Vector3.Dot(dir.normalized, ori.normalized);
            float otherDot = Vector3.Dot(cross.normalized, ori.normalized);
            //If the dot is greater than the cross, then we need to find the intersection point in the circle.
            if (dot > otherDot)
            {
                //1.Get a Vector "toC" from the origin to the center circle that make up that end of the capsual
                Vector3 toC = circlePoint - sPoint;
                //2.Dot "toC" onto direction.
                dot = Vector3.Dot(dir.normalized, toC);
                //3.Assuming "toC" is the hypotenuse and the adjacent is the Dot we just calculated, get the opposite.
                float opposite = Mathf.Sqrt((toC.magnitude * toC.magnitude) - (dot * dot));
                //4.Assuming the opposite we just calculated is the opposite and the radius of the circle is the hypotenuse, get the adjacent.
                opposite = Mathf.Sqrt((radius * radius) - (opposite * opposite));
                //5. return the Adjacent + Dot
                return dot + opposite;
            }
            //Otherwise we exit in the middle bit so we can use the dot and some unit circle math to resize dir to be what we want
            else
            {
                //First we need to get the length of the Opposite (dot is Adjacent)
                float opposite = Mathf.Sqrt(1 - (dot * dot));

                //Scale up Opposite so its length is radius
                float scale = radius / opposite;

                //Scale up Adjacent by the same amount
                dot *= scale;
                //A^2 + B^2 = C^2
                //return C
                return Mathf.Sqrt((dot * dot) + (opposite * opposite));
            }
        }
        /// <summary>
        /// Updates the location of the Collider in the game scene
        /// </summary>
        public void UpdateCollider()
        {   //Apply height, offset, rotation & scale.
            collider.radius = TrueRadius;
            collider.height = Height;
            //Get a point from the top of the collider and move it down by half the height
            collider.transform.position = GetHighestPoint() - Orientation * (Height / 2);
            collider.transform.rotation = Quaternion.LookRotation(collider.transform.forward, Orientation);
        }

        public override Vector3 GetClosestPoint(Vector3 point, Vector3 normal)
        {   //Make sure lineDir is normalized
            normal.Normalize();
            //Get the dot onto orientation
            float dot = Vector3.Dot(normal, Orientation);
            //If the dot is less than 0, then the direction must hit the upper point
            if (dot < 0)
                return GetUpperPoint() - normal * TrueRadius;
            //If its > 0 then it must hit the lower point
            else if (dot > 0)
                return GetLowerPoint() - normal * TrueRadius;
            //Get a vector from the origin to the lines origin and dot it onto orientation
            Vector3 oToLO = point - GetOriginPosition();
            dot = Vector3.Dot(oToLO, Orientation);
            //Move the origin of the collider along orientation by the dot to get the point it would hit
            return GetOriginPosition() + (Orientation * dot) - normal * TrueRadius;
        }

        protected override RaycastHit[] CastAllColliders(Vector3 castVector, Vector3 posOffset, float colliderOffset)
        {   //Perform the raycast
            RaycastHit[] h = Physics.CapsuleCastAll(GetUpperPoint() + posOffset, GetLowerPoint() + posOffset, radius, castVector.normalized, castVector.magnitude);
            //Sort the hit info by length so that the closest is first
            System.Array.Sort(h, Conditions.CompareDist);
            return h;
        }

        protected override RaycastHit CastCollider(Vector3 castVector, Vector3 posOffset, float colliderOffset)
        {
            Physics.CapsuleCast(GetUpperPoint() + posOffset, GetLowerPoint() + posOffset, radius, castVector.normalized, out RaycastHit h, castVector.magnitude);
            return h;
        }

#if UNITY_EDITOR
        public override void GizmosDrawCollider()
        {
            Gizmos.DrawWireSphere(GetOriginPosition() + Orientation * UpperHeight, Radius);
            Gizmos.DrawWireSphere(GetOriginPosition() - Orientation * LowerHeight, Radius);
        }
#endif
    }
}