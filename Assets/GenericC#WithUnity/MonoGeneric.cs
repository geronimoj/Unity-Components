
namespace GenericsEvents
{
    //In global namespace to make it easily accessible
    /// <summary>
    /// Inheritable class to gain access to IUnityEvents
    /// </summary>
    public class MonoGeneric : IUnityEvents
    {
        #region Construction & Destruction
        public MonoGeneric()
        {
            MasterGenericRunner.Instantiate(this);
        }

        ~MonoGeneric()
        {
            Unsubscribe();
        }
        #endregion
        /// <summary>
        /// Unsubscribes from the MasterGenericRunner. This is automatically called in the destructor/finalizer
        /// </summary>
        public void Unsubscribe() => MasterGenericRunner.UnSubscribe(this);
        /// <summary>
        /// Subscribes to the MasterGenericRunner. This is automatically done in the construction
        /// </summary>
        public void Subscribe() => MasterGenericRunner.Subscribe(this);

        #region Callbacks
        /// <summary>
        /// Called before the Start of all recently instanced MonoGenerics
        /// </summary>
        public virtual void Awake()
        {
        }
        /// <summary>
        /// Unity Fixed Update
        /// </summary>
        public virtual void FixedUpdate()
        {
        }
        /// <summary>
        /// Unity Late Update
        /// </summary>
        public virtual void LateUpdate()
        {
        }
        /// <summary>
        /// Called when this is created
        /// </summary>
        public virtual void OnSubscribe()
        {
        }
        /// <summary>
        /// Called when you manually unsubscribe from the MasterGenericRunner
        /// </summary>
        public virtual void OnUnSubscribe()
        {
        }

        /// <summary>
        /// Called before the Update of all recently instanced MonoGenerics
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// Unity Update
        /// </summary>
        public virtual void Update()
        {
        }
        #endregion
    }
}