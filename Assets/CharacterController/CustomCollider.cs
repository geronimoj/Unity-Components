using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{
    /// <summary>
    /// Collider that contains utility for validating changes to collider before applying & changing collider properties.
    /// </summary>
    /// <typeparam name="TColliderState"></typeparam>
    public abstract class CustomCollider
    {
        /// <summary>
        /// Small deduction taken away from Collision Offset when raycasting.
        /// </summary>
        /// <remarks>
        /// This resolves issues where moving perfectly against a surface would result in the outer
        /// collider registering as inside of the collider, instead of touching.
        /// </remarks>
        public const float TINY_DEDUCTION = 1e-5f;

        /// <summary>
        /// Layers the collider can collide with
        /// </summary>
        [SerializeField]
        protected LayerMask collisionLayers = int.MaxValue;
        /// <summary>
        /// Origin of collider in world
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
        protected Vector3 gravityDir = Physics.gravity.normalized;
        /// <summary>
        /// Origin of collider in world
        /// </summary>
        public Transform Origin => origin;
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
            get => gravityDir;
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
        public virtual void SetOrigin(Transform t)
        {
            origin = t;
        }
        /// <summary>
        /// Update the Unity Collider to match its current state
        /// </summary>
        public abstract void UpdateUnityCollider();
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
        /// <summary>
        /// Get the colliders that make up this custom colliders.
        /// </summary>
        /// <returns>Enumerable collection of colliders</returns>
        public abstract IEnumerable<Collider> GetColliders();
        /// <summary>
        /// Obtain any overlapping colliders
        /// </summary>
        /// <returns>A temporary array containing any overlapping colliders</returns>
        public abstract Collider[] GetOverlappingColliders();

        #region Raycasting

        #region Single Casts
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <returns>Returns true if something was hit</returns>
        public static bool Cast(CustomCollider c, Vector3 castVec)
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
        public static bool Cast(CustomCollider c, Vector3 castVec, Vector3 offset)
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
        public static bool Cast(CustomCollider c, Vector3 castVec, out RaycastHit hit)
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
        public static bool Cast(CustomCollider c, Vector3 castVec, Vector3 offset, out RaycastHit hit)
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
        public static bool CastWithOffset(CustomCollider c, Vector3 castVec)
        {
            return CastWithOffset(c, castVec, Vector3.zero, out _);
        }
        /// <summary>
        /// Performs a capsual cast using the given colliderInfo with collisionOffset
        /// </summary>
        /// <param name="c">The collider to cast</param>
        /// <param name="castVec">The direction and distance to cast the collider</param>
        /// <param name="offset">The offset from the origin the cast should take place</param>
        public static bool CastWithOffset(CustomCollider c, Vector3 castVec, Vector3 offset)
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
        public static bool CastWithOffset(CustomCollider c, Vector3 castVec, out RaycastHit hit)
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
        public static bool CastWithOffset(CustomCollider c, Vector3 castVec, Vector3 offset, out RaycastHit hit)
        {   //Perform the raycast
            hit = c.CastCollider(castVec, offset, c.CollisionOffset - TINY_DEDUCTION);
            //Return false if none of the hit results were valid
            return hit.distance != 0;
        }
        #endregion

        #region All Casts
        /// <summary>
        /// Performs a cast and returns infromation about everything hit
        /// </summary>
        /// <param name="c">The colliderInfo to cast</param>
        /// <param name="castVec">The distance and direction of the cast</param>
        /// <returns>Information about everything hit</returns>
        public static RaycastHit[] CastAll(CustomCollider c, Vector3 castVec)
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
        public static RaycastHit[] CastAll(CustomCollider c, Vector3 castVec, Vector3 offset)
        {
            return c.CastAllColliders(castVec, offset, 0);
        }
        /// <summary>
        /// Performs a cast with collisionOffset and returns infromation about everything hit
        /// </summary>
        /// <param name="c">The colliderInfo to cast</param>
        /// <param name="castVec">The distance and direction of the cast</param>
        /// <returns>Information about everything hit</returns>
        public static RaycastHit[] CastAllWithOffset(CustomCollider c, Vector3 castVec)
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
        public static RaycastHit[] CastAllWithOffset(CustomCollider c, Vector3 castVec, Vector3 offset)
        {
            return c.CastAllColliders(castVec, offset, c.CollisionOffset - TINY_DEDUCTION);
        }
        #endregion

        #region Non Alloc Casts
        /// <summary>
        /// Performs a cast and returns infromation about everything hit
        /// </summary>
        /// <param name="c">The colliderInfo to cast</param>
        /// <param name="castVec">The distance and direction of the cast</param>
        /// <returns>Information about everything hit</returns>
        public static int CastAllNonAlloc(CustomCollider c, Vector3 castVec, RaycastHit[] hits)
        {
            return CastAllNonAlloc(c, castVec, Vector3.zero, hits);
        }
        /// <summary>
        /// Performs a cast and return everything that was hit
        /// </summary>
        /// <param name="c">The collider info to cast</param>
        /// <param name="castVec">The direction and distance of the cast</param>
        /// <param name="offset">The positional offset of the cast</param>
        /// <returns>Hit information about everything that was hit</returns>
        public static int CastAllNonAlloc(CustomCollider c, Vector3 castVec, Vector3 offset, RaycastHit[] hits)
        {
            return c.CastAllCollidersNonAlloc(castVec, offset, 0, hits);
        }
        /// <summary>
        /// Performs a cast with collisionOffset and returns infromation about everything hit
        /// </summary>
        /// <param name="c">The colliderInfo to cast</param>
        /// <param name="castVec">The distance and direction of the cast</param>
        /// <returns>Information about everything hit</returns>
        public static int CastAllWithOffsetNonAlloc(CustomCollider c, Vector3 castVec, RaycastHit[] hits)
        {
            return CastAllWithOffsetNonAlloc(c, castVec, Vector3.zero, hits);
        }
        /// <summary>
        /// Performs a cast with collisionOffset and return everything that was hit
        /// </summary>
        /// <param name="c">The collider info to cast</param>
        /// <param name="castVec">The direction and distance of the cast</param>
        /// <param name="offset">The positional offset of the cast</param>
        /// <returns>Hit information about everything that was hit</returns>
        public static int CastAllWithOffsetNonAlloc(CustomCollider c, Vector3 castVec, Vector3 offset, RaycastHit[] hits)
        {
            return c.CastAllCollidersNonAlloc(castVec, offset, c.CollisionOffset - TINY_DEDUCTION, hits);
        }
        #endregion

        #region Abstract
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
        /// Raycasts the collider along castVector and returns all colliders hit. Results will be stored by distance afterwards. Avoids memory allocation.
        /// </summary>
        /// <param name="castVector">The direction and distance to raycast the collider</param>
        /// <param name="posOffset">The offset from the colliders current position in world position</param>
        /// <param name="colliderOffset">A size increase to the collider. Imagine extruding every face of the collider out by this amount before raycasting</param>
        /// <param name="raycastHits">Maxiumum number of hits to avoid allocation</param>
        /// <returns>Returns the raycastHit information of all colliders hit</returns>
        /// <remarks>This is primarily used when projecting the collider for movement. When projecting, the collider is projected twice. Once
        /// normally and a second time with the collisionOffset applied (colliderOffset).</remarks>
        protected abstract int CastAllCollidersNonAlloc(Vector3 castVector, Vector3 posOffset, float colliderOffset, RaycastHit[] raycastHits);
        /// <summary>
        /// Raycasts the collider along castVector and returns the first collider hit.
        /// </summary>
        /// <param name="castVector">The direction and distance to raycast the collider</param>
        /// <param name="posOffset">The offset from the colliders current position in world position</param>
        /// <param name="colliderOffset">A size increase to the collider. Imagine extruding every face of the collider out by this amount before raycasting</param>
        /// <returns>Returns the raycastHit information of the collider hit. Returns default raycastHit if not collider was hit</returns>
        protected abstract RaycastHit CastCollider(Vector3 castVector, Vector3 posOffset, float colliderOffset);
        #endregion

        #endregion

#if UNITY_EDITOR
        public void EDITOR_SetOrigin(Transform t)
        {
            origin = t;
        }
        /// <summary>
        /// Draws the collider in gizmos. Note: This is editor only
        /// </summary>
        public virtual void EDITOR_GizmosDrawCollider() { }
#endif
    }

    public abstract class ValidatableCollider<TColliderInfo> : CustomCollider
    {
        #region Collider Changing
        /// <summary>
        /// Get the data that defines the colliders current shape
        /// </summary>
        /// <returns>Object containing the current collider state</returns>
        public abstract TColliderInfo GetColliderInfo();
        /// <summary>
        /// Update this collider to be identical to another collider
        /// </summary>
        /// <param name="colliderData">The data to copy from</param>
        public abstract void ApplyColliderInfo(TColliderInfo toCopy);
        /// <summary>
        /// Compares the colliders current state to the provided state, checking if the collider changes could be made without intersecting without intersecting with geometry
        /// </summary>
        /// <param name="toCompare">The collider to simulate changing into</param>
        /// <param name="applyOnSuccess">Apples the target collider info if changes can be made without intersecting with geometry</param>
        /// <param name="failReason">Reason the collider failed to be changed</param>
        /// <returns>True if the changes can be made without intersecting with geometry. False otherwise.</returns>
        public abstract bool ValidateColliderChanges(TColliderInfo toCompare, bool applyOnSuccess, out int failReason);
        #endregion
    }
}