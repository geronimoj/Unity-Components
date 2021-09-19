using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CustomPhysics.Joints
{
    /// <summary>
    /// Custom joint for use with grappling
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(100)]
    public class GrappleJoint : MonoBehaviour
    {
        private Rigidbody _rb = null;
        /// <summary>
        /// The point we are anchored to
        /// </summary>
        [SerializeField]
        private Vector3 _anchorPoint = Vector3.zero;
        /// <summary>
        /// The point we are anchored to
        /// </summary>
        public Vector3 AnchorPoint
        {
            get => _anchorPoint;
            set
            {
                _anchorPoint = value;
            }
        }
        /// <summary>
        /// Should this script automatically adjust max radius
        /// </summary>
        [SerializeField]
        [Tooltip("Should the maxRadius be reduced if this object gets closer to anchor point")]
        private bool _autoShrinkRadius = false;
        /// <summary>
        /// Should the GrappleJoint adjust the max radius if the object gets closer to the anchor point.
        /// </summary>
        public bool AutoShrinkRadius
        {
            get => _autoShrinkRadius;
            set => _autoShrinkRadius = value;
        }
        /// <summary>
        /// The maximum distance this object can be from the grapple point
        /// </summary>
        [SerializeField]
        [Tooltip("The maximum distance this object can be from the grapple point")]
        private float _maxRadius = 0;
        /// <summary>
        /// The maximum distance this object can be from the grapple point
        /// </summary>
        public float MaxRadius
        {
            get => _maxRadius;
            set
            {
                if (value <= 0)
                    return;

                _maxRadius = value;
            }
        }
        /// <summary>
        /// Gets reference to the rigidbody
        /// </summary>
        private void Awake() =>
            _rb = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            //Calculate if our movement would overshoot the radius (we go further away than the max radius)
            Vector3 newPoint = transform.position + _rb.velocity * Time.fixedDeltaTime;
            //This is tehcniqually memory inefficient as we don't use toAnchor unless dist > maxRadius however it is ever so faster on the CPU not having to calculate the same vector twice
            Vector3 toAnchor = _anchorPoint - newPoint;
            float dist = toAnchor.magnitude;
            //If so, apply a force towards the anchor point that results in us ending our movment on the edge of the radius
            if (dist > _maxRadius)
            {   //Calculate how much we over shot the raidus
                float overShoot = dist - _maxRadius;
                //Calcualte the force to apply to the rigidbody
                toAnchor = toAnchor.normalized * overShoot;
                //UPODATE
                _rb.velocity += toAnchor / Time.fixedDeltaTime;
            }
            //If not, if _autoShrinkRadius is true, re-calculate the distance
            else if (_autoShrinkRadius)
                //Changed the radius
                MaxRadius = dist;
        }
    }
}