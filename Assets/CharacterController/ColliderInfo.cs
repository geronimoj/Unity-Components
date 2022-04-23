using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{
    /// <summary>
    /// Base class for all colliders
    /// </summary>
    public abstract class ColliderInfo
    {
        /// <summary>
        /// A reference to the players transform
        /// </summary>
        protected Transform origin = null;
        /// <summary>
        /// Is true if the collider is on the ground
        /// </summary>
        protected bool onGround = false;
        /// <summary>
        /// The distance away from any surface the collider should remain. CANNOT BE LESS THAN OR EQUAL TO 0
        /// </summary>
        [SerializeField]
        [Range(0.00001f, 1f)]
        [Tooltip("The distance away from any surface the collider should remain. Cannot be LESS THAN OR EQUAL TO 0")]
        protected float collisionOffset;
        /// <summary>
        /// The angle at which a slope is considered ground the player can walk up.
        /// </summary>
        [Tooltip("The angle of a slope at which the player can walk up. Angles less than this value are considered ground")]
        [SerializeField]
        [Range(0, 90)]
        protected float slopeAngle = 30;
        /// <summary>
        /// The direction of gravity
        /// </summary>
        protected Vector3 gravityDir = Vector3.zero;
        /// <summary>
        /// A Get Set for OnGround. When the PlayerController is moved, this value is updated.
        /// </summary>
        /// <remarks>Is true when the controller is on the ground</remarks>
        public bool OnGround
        {
            get => onGround;
            set => onGround = value;
        }
        /// <summary>
        /// A Get Set for the slopeAngle
        /// SlopeAngle is the angle of a surface that the player can walk up and is considered ground
        /// </summary>
        public float SlopeAngle
        {
            get => slopeAngle;
            set
            {   //Make sure value is between 0 and 90 degrees
                if (value > 0 && value < 90)
                    slopeAngle = value;
            }
        }
        /// <summary>
        /// Gets or Sets the direction of gravity. If gravityDir is zero, (0, -1, 0) is returned
        /// </summary>
        public Vector3 GravityDirection
        {
            get
            {   //If gravityDir is 0 or unassigned return(0, -1, 0)
                if (gravityDir == Vector3.zero)
                    GravityDirection = Physics.gravity;
                return gravityDir;
            }
            set
            {
                gravityDir = value.normalized;
            }
        }
        /// <summary>
        /// A Get Set for the collision offset.
        /// Collision Offset is the distance the collider must be from any surface
        /// </summary>
        public float CollisionOffset
        {
            get
            {   //If collisionOffset is 0, or worse, negative, return 0.01f.
                if (collisionOffset <= 0)
                    return 0.01f;
                return collisionOffset;
            }
            set
            {   //If value is < 0. Set collision offset to 0.01f
                if (value < 0)
                    value = 0.01f;
                collisionOffset = value;
            }
        }
        /// <summary>
        /// Sets reference to the transform to the given transform.
        /// </summary>
        /// <param name="t">The transform to be set</param>
        public virtual void SetTransform(Transform t)
        {
            origin = t;
            //Spit out an error if the transform is invalid
            CheckValidTransform();
        }
        /// <summary>
        /// Checks if the transform locally stored is valid
        /// </summary>
        /// <returns>Returns true if this transform is invalid</returns>
        protected bool CheckValidTransform()
        {   //If origin is null, report and error and break
            if (origin == null)
            {
                Debug.LogError("Transform not assigned to collider info");
                Debug.Break();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Determines if a slope given by a normal can be stood on or is too steep
        /// </summary>
        /// <param name="normal">The normal of the slope</param>
        /// <returns>Returns true if the player can stand on the surface</returns>
        public bool ValidSlope(Vector3 normal)
        {
            if (Vector3.Dot(normal, -GravityDirection) >= Mathf.Cos(SlopeAngle * Mathf.Deg2Rad))
                return true;
            return false;
        }
        /// <summary>
        /// Given a point and a normal, return the closest point on the edge of the collider to the point
        /// </summary>
        /// <param name="point">The point in world space</param>
        /// <param name="normal">The normal of the point</param>
        /// <returns>The closest point to point on the surface of the collider.</returns>
        /// <remarks>This is used for calculating the closest point to a collision. Concave colliders may require extra checks.</remarks>
        public abstract Vector3 GetClosestPoint(Vector3 point, Vector3 normal);
        /// <summary>
        /// Gets the lowest point on the collider
        /// </summary>
        /// <returns>The lowest point on the collider</returns>
        public abstract Vector3 GetLowestPoint();

        #region Raycasting
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool Cast(ColliderInfo c, Vector3 castVec)
        {
            return Cast(c, castVec, Vector3.zero, out _);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="offset">The offset from the origin the cast should take place</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool Cast(ColliderInfo c, Vector3 castVec, Vector3 offset)
        {
            return Cast(c, castVec, offset, out _);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="hit">Infromation about the object that was hit</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool Cast(ColliderInfo c, Vector3 castVec, out RaycastHit hit)
        {
            return Cast(c, castVec, Vector3.zero, out hit);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="offset">The offset from origin the cast should take place</param>
        /// <param name="hit">Information about the object that was hit</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool Cast(ColliderInfo c, Vector3 castVec, Vector3 offset, out RaycastHit hit)
        {   //Perform the raycast and get the results
            hit = c.CastCollider(castVec, offset, 0);
            return hit.distance != 0;
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo with their collisionOffset
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool CastWithOffset(ColliderInfo c, Vector3 castVec)
        {
            return CastWithOffset(c, castVec, Vector3.zero, out _);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo with collisionOffset
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="offset">The offset from the origin the cast should take place</param>
        public static bool CastWithOffset(ColliderInfo c, Vector3 castVec, Vector3 offset)
        {
            return CastWithOffset(c, castVec, offset, out _);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo with collisionOffset
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="hit">Infromation about the object that was hit</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool CastWithOffset(ColliderInfo c, Vector3 castVec, out RaycastHit hit)
        {
            return CastWithOffset(c, castVec, Vector3.zero, out hit);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo with collisionOffset
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="offset">The offset from origin the cast should take place</param>
        /// <param name="hit">Information about the object that was hit</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool CastWithOffset(ColliderInfo c, Vector3 castVec, Vector3 offset, out RaycastHit hit)
        {   //Perform the raycast
            hit = c.CastCollider(castVec, offset, c.CollisionOffset);
            //Return false if none of the hit results were valid
            return hit.distance != 0;
        }
        /// <summary>
        /// Performs a cast and returns infromation about everything hit
        /// </summary>
        /// <param name="c">The colliderInfo to cast</param>
        /// <param name="castVec">The distance and direction of the cast</param>
        /// <returns>Information about everything hit</returns>
        public static RaycastHit[] CastAll(ColliderInfo c, Vector3 castVec)
        {
            return CastAll(c, castVec, Vector3.zero);
        }
        /// <summary>
        /// Performs a cast and return everything that was hit
        /// </summary>
        /// <param name="c">The collider info to cast</param>
        /// <param name="castVec">The direction and distance of the cast</param>
        /// <param name="offset">The positional offset of the cast</param>
        /// <returns>Hit information about everything that was hit</returns>
        public static RaycastHit[] CastAll(ColliderInfo c, Vector3 castVec, Vector3 offset)
        {
            return c.CastAllColliders(castVec, offset, 0);
        }
        /// <summary>
        /// Performs a cast with collisionOffset and returns infromation about everything hit
        /// </summary>
        /// <param name="c">The colliderInfo to cast</param>
        /// <param name="castVec">The distance and direction of the cast</param>
        /// <returns>Information about everything hit</returns>
        public static RaycastHit[] CastAllWithOffset(ColliderInfo c, Vector3 castVec)
        {
            return CastAllWithOffset(c, castVec, Vector3.zero);
        }
        /// <summary>
        /// Performs a cast with collisionOffset and return everything that was hit
        /// </summary>
        /// <param name="c">The collider info to cast</param>
        /// <param name="castVec">The direction and distance of the cast</param>
        /// <param name="offset">The positional offset of the cast</param>
        /// <returns>Hit information about everything that was hit</returns>
        public static RaycastHit[] CastAllWithOffset(ColliderInfo c, Vector3 castVec, Vector3 offset)
        {
            return c.CastAllColliders(castVec, offset, c.CollisionOffset);
        }
        /// <summary>
        /// Raycasts the collider along castVector and returns all colliders hit. Results will be stored by distance afterwards
        /// </summary>
        /// <param name="castVector">The direction and distance to raycast the collider</param>
        /// <param name="posOffset">The offset from the colliders current position in world position</param>
        /// <param name="colliderOffset">A size increase to the collider. Imagine extruding every face of the collider out by this amount before raycasting</param>
        /// <returns>Returns the raycastHit information of all colliders hit</returns>
        /// <remarks>This is primarily used when projecting the collider for movement. When projecting, the collider is projected twice. Once
        /// normally and a second time with the collisionOffset applied (colliderOffset).</remarks>
        protected abstract RaycastHit[] CastAllColliders(Vector3 castVector, Vector3 posOffset, float colliderOffset);
        /// <summary>
        /// Raycasts the collider along castVector and returns the first collider hit.
        /// </summary>
        /// <param name="castVector">The direction and distance to raycast the collider</param>
        /// <param name="posOffset">The offset from the colliders current position in world position</param>
        /// <param name="colliderOffset">A size increase to the collider. Imagine extruding every face of the collider out by this amount before raycasting</param>
        /// <returns>Returns the raycastHit information of the collider hit. Returns default raycastHit if not collider was hit</returns>
        protected abstract RaycastHit CastCollider(Vector3 castVector, Vector3 posOffset, float colliderOffset);
        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Draws the collider in gizmos. Note: This is editor only
        /// </summary>
        public abstract void GizmosDrawCollider();
#endif
    }
}