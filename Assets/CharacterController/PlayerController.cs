using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomController
{
    /// <summary>
    /// A Controller with the CapsualCollider as its type of collider
    /// </summary>
    public class PlayerController : Controller<CapsualCollider> { }
    /// <summary>
    /// Contains all the information for the player. Contains code that should be used to Move and rotate the player
    /// </summary>
    /// <remarks>Optional Defines: COLLISION_CALL_IMMEDIATE - Determines if to invoke OnCollision event after calculating for collisions or while</remarks>
    public class Controller<T> : MonoBehaviour where T : ColliderInfo
    {
        private const int MAX_ATTEMPTS = 100;
        /// <summary>
        /// The collider used for this PlayerController
        /// </summary>
        public T colInfo = null;

        public MovementDirection direction;

#if UNITY_EDITOR
        /// <summary>
        /// Used for debugging information
        /// </summary>
        private Vector3 _moveVec = Vector3.zero;
#endif
        /// <summary>
        /// The colliders that should be ignored when moving. This adds child colliders by default
        /// </summary>
        public HashSet<Collider> collidersToIgnore = null;
        /// <summary>
        /// Everything that was collided with in the previous movement
        /// </summary>
        private readonly List<RaycastHit> _collidedWith = new List<RaycastHit>();
        /// <summary>
        /// Used to track raycasts. Since the data does not need to persist, we can make this static.
        /// </summary>
        private static readonly Queue<RaycastHit> _temporary = new Queue<RaycastHit>();
        /// <summary>
        /// Called on every collision that has to be calculated.
        /// </summary>
        /// <remarks>Collision data can also be obtained from CollidedWith</remarks>
        public event EventHandler<RaycastHit> OnCollision;
        /// <summary>
        /// The objects that were collided with in the previous movement.
        /// </summary>
        public List<RaycastHit> CollidedWith => _collidedWith;
        /// <summary>
        /// Is true if the controller is on the ground
        /// </summary>
        public bool OnGround
        {
            get { return colInfo.OnGround; }
        }
        /// <summary>
        /// Sets the transform and adds child colliders to colliders to ignore
        /// </summary>
        private void Start()
        {
            colInfo.SetTransform(transform);
            collidersToIgnore = new HashSet<Collider>(GetComponentsInChildren<Collider>());
        }
        /// <summary>
        /// Moves the player. Contains all collision detection required
        /// </summary>
        /// <param name="dir">The direction with a magnitude of distance the player should be moved</param>
        /// <param name="cancelOnFail">If true, the player will not be moved if dir is changed</param>
        public void Move(Vector3 dir, bool cancelOnFail = false)
        {
#if UNITY_EDITOR
            _moveVec = dir;
#endif      //Clear what we collided with
            _collidedWith.Clear();
            //Make sure we have a direction to move in
            //Otherwise just update if we are on the ground
            if (dir != Vector3.zero)
            {
                //Get the collisions
                MoveToRaycasts(dir
#if UNITY_EDITOR
                    , out int i
#endif
                    );

                RaycastHit hit;
                int attempts = 0;
                //Loop through the collisions and adjust the current movement direction so that it is not pointing into any normals
                while (_temporary.Count > 0)
                {   
                    attempts++;
                    if (attempts > MAX_ATTEMPTS)
                    {
                        Debug.LogError("Could not solve for collision response. Not moving character");
                        return;
                    }

                    hit = _temporary.Dequeue();

                    //Make sure aren't colliding with ourself. This does in turn mean that objects we are too close too cannot be collided with
                    if (hit.distance != 0)
                    {
                        float dot = Vector3.Dot(hit.normal, dir);
                        //Make sure we are heading into the normal
                        if (dot < 0)
                        {   //If we can't move there, exit if this bool is true
                            if (cancelOnFail)
                                return;

                            //Get a vector from the point we collided with, to he closest point on the surface of the collider
                            Vector3 curNew = colInfo.GetClosestPoint(hit.point, hit.normal) - hit.point;
                            //Get the dot product against the normal (its literally dot that we calculated earlier but positive)
                            dot = Mathf.Abs(dot);
                            //Subtract the dot product from are calculated vector to only get the overshooting amount.
                            dot -= Vector3.Dot(curNew, hit.normal);
                            //One last check that we 100% are definately colliding with the surface
                            //If Dot is < 0, then we would be moved into the wall rather than away.
                            if (dot > 0)
                            {
                                //We then apply this as a vector along the normal of the hit surface with a bit of extra stuff to adjust the movement vector away from the wall
                                dir += hit.normal * (dot);
                                //Store what we collided with
                                _collidedWith.Add(hit);
#if COLLISION_CALL_IMMEDIATE
                                //Invoke the event
                                OnCollision?.Invoke(this, hit);
#endif
                                //Tell ourself to update the movement information
                                MoveToRaycasts(dir
#if UNITY_EDITOR
                                    , out i
#endif
                                    );
#if UNITY_EDITOR
                                i++;
#endif
                            }
                        }
                    }
#if UNITY_EDITOR
                    else
                    {
                        if (i < 0)
                            //If this debug message is being called, we have a big problem
                            Debug.LogWarning("Too close to surface: " + hit.transform.gameObject.name);
                        else
                            //This Debug message is ok to see
                            Debug.LogWarning("In collider: " + hit.transform.gameObject.name);
                    }
                    //The hit data shrinked so we are getting closer to checking the surface raycasts
                    i--;
#endif
                }
#if UNITY_EDITOR
                {
                    //Draw the movement and then move us along it
                    Vector3 lowest = colInfo.GetLowestPoint();
                    Debug.DrawLine(lowest, lowest + dir, Color.magenta, 20f);
                }
#endif          //MOVE
                transform.Translate(dir, Space.World);
#if !COLLISION_CALL_IMMEDIATE

                if (OnCollision != null)
                    //Invoke the event afterwards
                    foreach(RaycastHit h in _temporary)
                        OnCollision(this, h);
#endif

            }
            //Do a raycast down to check if we are on the ground
            MoveToRaycasts(colInfo.GravityDirection * 1e-3f
#if UNITY_EDITOR
                , out int _
#endif
                );
            //Loop through the downwards raycast results and check if any of them meet the on ground conditions
            foreach(RaycastHit hit in _temporary)
                if (colInfo.ValidSlope(hit.normal))
                {   //If we just entered onGround, then change our movement vector to be that direction
                    //This exists to help with exiting from slopes we can't stand on. However, in turn, its causes other issues with going up small un-ramped bumps, 
                    //causing the player to drift on them. So we also check that the point we hit is below us, so that small ledges at our height aren't affected
                    if (!colInfo.OnGround && (hit.point - colInfo.GetLowestPoint()).y < 0)
                        direction.HozDirection = dir.normalized;
                    //We found one of them so set us to be on the ground, and set previous ground
                    colInfo.OnGround = true;
                    return;
                }
            //Set us to not be on the ground. We can only hit this if the previous checks failed
            colInfo.OnGround = false;
        }
        /// <summary>
        /// Moves the player. Contains all collision detection required.
        /// OBSELETE. Use Move instead. Updating function names. Only exists to not break code
        /// </summary>
        /// <param name="dir">The direction with a magnitude of distance the player should be moved</param>
        /// <param name="cancelOnFail">If true, the player will not be moved if dir is changed</param>
        [Obsolete("Use Move instead. Because this, infact, does not teleport you...")]
        public void MoveTo(Vector3 dir, bool cancelOnFail = false) => Move(dir, cancelOnFail);
        /// <summary>
        /// Performs the raycasts to detect collisions for MoveTo
        /// </summary>
        /// <param name="dir">The direction & magnitude of the raycasts</param>
        /// <param name="offsetIndex">The index that the output hitInfo is from the second raycast instead of the first</param>
        /// <returns>An array containing hitInfo from two raycasts. One with radius, the other with radius + offset</returns>
        private void MoveToRaycasts(Vector3 dir
#if UNITY_EDITOR
            , out int offsetIndex
#endif
            )
        {   //Raycast for the players regular collider
            RaycastHit[] regular = ColliderInfo.CastAll(colInfo, dir);
            //Raycast for the players collider with the offset
            RaycastHit[] withOffset = ColliderInfo.CastAllWithOffset(colInfo, dir);
            //Remove the colliders we are supposed to ignore
            //This is inefficient and should be solved
            //Combine the raycast results
#if UNITY_EDITOR
            offsetIndex = 0;
#endif
            _temporary.Clear();
            //Repeat for the offset checks
            foreach (RaycastHit h in withOffset)
                if (!collidersToIgnore.Contains(h.collider))
                {
                    _temporary.Enqueue(h);
#if UNITY_EDITOR
                    offsetIndex++;
#endif
                }
            //Make sure hit colliders are not to be ignored
            foreach (RaycastHit h in regular)
                if (!collidersToIgnore.Contains(h.collider))
                    _temporary.Enqueue(h);
        }

#region UNITYEDITOR
#if UNITY_EDITOR
        /// <summary>
        /// Draw debugging information
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            colInfo.SetTransform(transform);
            Gizmos.color = Color.green;

            colInfo.GizmosDrawCollider();

            Gizmos.DrawLine(transform.position, transform.position + _moveVec);
        }
#endif
#endregion
    }
}