using UnityEngine;

namespace CustomPhysics.Joints
{
    /// <summary>
    /// Custom joint for use with grappling
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(1000)]
    public class GrappleJoint : MonoBehaviour
    {
        /// <summary>
        /// The rigidbody we are connected to
        /// </summary>
        [SerializeField]
        [Tooltip("The rigidbody that this joint should manipulate")]
        private Rigidbody _rigidBody = null;
        /// <summary>
        /// The point we are anchored to
        /// </summary>
        [Tooltip("The point the grapple joint is connected to.")]
        private Vector3 _anchorPoint = Vector3.zero;
        /// <summary>
        /// Local connection point
        /// </summary>
        [SerializeField]
        [Tooltip("The point the grapple joint connects to on the rigidbody in local space")]
        private Vector3 _localConnection = Vector3.zero;
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
        [Tooltip("Should the max radius be reduced if this object gets closer to anchor point")]
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
            {   //Do not allow negative or 0 radius
                if (value <= 0)
                    return;

                _maxRadius = value;
            }
        }
        /// <summary>
        /// Gets reference to the rigidbody if one was not assigned manually
        /// </summary>
        [ContextMenu("Auto Setup")]
        private void Awake() 
        {   //If no rigidbody has been manually assigned, attempt to find one
            if (!_rigidBody)
            {
                _rigidBody = GetComponent<Rigidbody>();
                //If its still null, report an error
                if (!_rigidBody)
                {
                    Debug.LogError("Could not find Rigidbody for GrappleJoint on object " + gameObject.name + ". Disabling GrappleJoint to avoid additional errors.");
                    //Disable the component so that the fixed update does not run and cause additional errors
                    enabled = false;
                }
            }
        }
        /// <summary>
        /// Updates the movement vector of the rigid body
        /// </summary>
        private void FixedUpdate()
        {   //Storage for relative point
            Vector3 relativePoint = transform.position;
            //If we have a local connection that isn't vector3.zero
            if (_localConnection != Vector3.zero)
                //Calculate the relative point on the object
                relativePoint += transform.forward * _localConnection.z + transform.right * _localConnection.x + transform.up * _localConnection.y;
            //Calculate if our movement would overshoot the radius (we go further away than the max radius)
            Vector3 newPoint = relativePoint + _rigidBody.velocity * Time.fixedDeltaTime;
            //This is tehcniqually memory inefficient as we don't use toAnchor unless dist > maxRadius however it is ever so faster on the CPU not having to calculate the same vector twice
            Vector3 toAnchor = _anchorPoint - newPoint;
            float dist = toAnchor.magnitude;
            //If so, apply a force towards the anchor point that results in us ending our movment on the edge of the radius
            if (dist > _maxRadius)
            {   //Calcualte the force to apply to the rigidbody
                toAnchor = toAnchor.normalized
                    //Calculate how much we over shot the radius
                    * (dist - _maxRadius);
                //UPODATE
                _rigidBody.velocity += toAnchor / Time.fixedDeltaTime;
            }
            //If not, if _autoShrinkRadius is true, re-calculate the distance
            else if (_autoShrinkRadius)
                //Changed the radius
                MaxRadius = dist;
        }
    }
}