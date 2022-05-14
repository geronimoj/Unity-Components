using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenericsEvents
{
    /// <summary>
    /// Provides generic c# scripts access to Update and related functions. This class is automated and spawns itself at runtime.
    /// </summary>
    public class MasterGenericRunner : MonoBehaviour
    {
        /// <summary>
        /// Has a master generic runner already spawned
        /// </summary>
        private static bool s_spawned = false;
        /// <summary>
        /// Is the master generic runner currently running over a foreach loop
        /// </summary>
        private static bool s_looping = false;
#if ORDERED_GENERICS
    private static List<IUnityEvents> s_generics = new List<IUnityEvents>();
#else
        /// <summary>
        /// IUnityEvents to call Update on
        /// </summary>
        private static HashSet<IUnityEvents> s_generics = new HashSet<IUnityEvents>();
#endif
        /// <summary>
        /// IUnityEvents to call Awake and Start on
        /// </summary>
        private static List<IUnityEvents> s_toInitialize = new List<IUnityEvents>();
        /// <summary>
        /// IUnityEvents that need to be unsubscribed but we are currently in a foreach loop so its not possible
        /// </summary>
        private static Queue<IUnityEvents> s_toRemove = new Queue<IUnityEvents>();
        /// <summary>
        /// Subscribe to start recieving Update calls
        /// </summary>
        /// <param name="obj"></param>
        public static void Subscribe(IUnityEvents obj)
        {
            if (obj == null || s_generics.Contains(obj))
                return;

            try
            {
                obj.OnSubscribe();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            s_generics.Add(obj);
        }
        /// <summary>
        /// Unsubscribe to stop recieving Update calls
        /// </summary>
        /// <param name="obj"></param>
        public static void UnSubscribe(IUnityEvents obj)
        {
            if (obj == null || !s_generics.Contains(obj))
                return;

            try
            {
                obj.OnUnSubscribe();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            //Currently in a foreach loop so we cannot edit the data types yet.
            if (s_looping)
            {
                s_toRemove.Enqueue(obj);
                return;
            }

            s_generics.Remove(obj);
        }
        /// <summary>
        /// Queues for Awake & Start calls before subscribing for Update calls
        /// </summary>
        /// <param name="obj"></param>
        public static void Instantiate(IUnityEvents obj)
        {
            if (obj == null || s_generics.Contains(obj))
                return;

            obj.OnSubscribe();
            s_toInitialize.Add(obj);
        }
        /// <summary>
        /// Initializes any queued objects for Awake & Start
        /// </summary>
        private static void Initialize()
        {
            if (s_toInitialize.Count == 0)
                return;

            s_looping = true;
            //Awake
            foreach (IUnityEvents obj in s_toInitialize)
            {
                try
                {
                    obj.Awake();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            //Start
            foreach (IUnityEvents obj in s_toInitialize)
            {
                try
                {
                    obj.Start();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                //Store as its initialized
                s_generics.Add(obj);
            }
            s_looping = false;
            UnSubscribeAfterForeach();
        }
        /// <summary>
        /// This exists as a catch to avoid unsubscribing during foreach loop which would break foreach loop 
        /// since you are not allowed to change the contents of IEnumeratable during foreach
        /// </summary>
        private static void UnSubscribeAfterForeach()
        {   //Don't want to remove while in foreach loop
            if (s_looping)
                return;
            //Unsubscribe all
            IUnityEvents e;
            while (s_toRemove.Count > 0)
            {
                e = s_toRemove.Dequeue();
                s_generics.Remove(e);
            }
        }

        #region UNITY FUNCTIONS
        private void Awake()
        {   //Spawned through runtime initializeOnLoad
            if (s_spawned)
                return;

            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {   //Initialize any queued objects
            Initialize();
            //Do Update
            s_looping = true;
            foreach (IUnityEvents obj in s_generics)
            {
                try
                {
                    obj.Update();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            //Remove any IunityEvents that have been slated for removal
            s_looping = false;
            UnSubscribeAfterForeach();
        }

        private void FixedUpdate()
        {   //Don't allow direct removal of IUnityEvents from s_generics
            s_looping = true;
            foreach (IUnityEvents obj in s_generics)
            {
                try
                {
                    obj.FixedUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            //Remove any IunityEvents that have been slated for removal
            s_looping = false;
            UnSubscribeAfterForeach();
        }

        private void LateUpdate()
        {   //Don't allow direct removal of IUnityEvents from s_generics
            s_looping = true;
            foreach (IUnityEvents obj in s_generics)
            {
                try
                {
                    obj.LateUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            //Remove any IunityEvents that have been slated for removal
            s_looping = false;
            UnSubscribeAfterForeach();
        }
        #endregion

        #region AUTO SETUP
        /// <summary>
        /// Spawns a master generic runner at runtime to remove the need to spawn it manually.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Setup()
        {   //Avoid double spawn
            if (s_spawned)
                return;
            //Spawn a runner and put it in dont destroy on load
            DontDestroyOnLoad(new GameObject("MasterGenericRunner", typeof(MasterGenericRunner)));
            s_spawned = true;
        }
        #endregion
    }
    /// <summary>
    /// Interface with function callbacks
    /// </summary>
    public interface IUnityEvents
    {
        void Awake();
        void Start();
        void Update();
        void FixedUpdate();
        void LateUpdate();
        void OnSubscribe();
        void OnUnSubscribe();
    }
}