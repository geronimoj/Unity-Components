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
        /// <summary>
        /// IUnityEvents to call Update, Fixed & LateUpdate on
        /// </summary>
#if ORDERED_GENERICS
        private static List<Wrapper> s_generics = new List<Wrapper>();
#else
        private static HashSet<IUnityEvents> s_generics = new HashSet<IUnityEvents>();
#endif
        /// <summary>
        /// IUnityEvents to call Awake and Start on
        /// </summary>
#if ORDERED_GENERICS
        private static List<Wrapper> s_toInitialize = new List<Wrapper>();
#else
        private static List<IUnityEvents> s_toInitialize = new List<IUnityEvents>();
#endif
        /// <summary>
        /// IUnityEvents that need to be unsubscribed but we are currently in a foreach loop so its not possible
        /// </summary>
        private static Queue<IUnityEvents> s_toRemove = new Queue<IUnityEvents>();
        /// <summary>
        /// Subscribe to start recieving Update calls
        /// </summary>
        /// <param name="obj"></param>
        public static void Subscribe(IUnityEvents obj
#if ORDERED_GENERICS
            , int executionOrder = 0
#endif
            )
        {
            if (obj == null || Contains(obj))
                return;

            try
            {
                obj.OnSubscribe();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
#if ORDERED_GENERICS
            Wrapper w = new Wrapper(obj, executionOrder);
            Add(w, s_generics);
#else
            s_generics.Add(obj);
#endif
        }
        /// <summary>
        /// Unsubscribe to stop recieving Update calls
        /// </summary>
        /// <param name="obj"></param>
        public static void UnSubscribe(IUnityEvents obj)
        {
            if (obj == null || !Contains(obj))
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
#if ORDERED_GENERICS
            Remove(obj, s_generics);
#else
            s_generics.Remove(obj);
#endif
        }
        /// <summary>
        /// Queues for Awake & Start calls before subscribing for Update calls
        /// </summary>
        /// <param name="obj"></param>
        public static void Instantiate(IUnityEvents obj
#if ORDERED_GENERICS
                                    , int executionOrder = 0
#endif
            )
        {
            if (obj == null || Contains(obj))
                return;

            obj.OnSubscribe();
#if ORDERED_GENERICS
            Wrapper w = new Wrapper(obj, executionOrder);
            Add(w, s_toInitialize);
#else
            s_toInitialize.Add(obj);
#endif
        }
        /// <summary>
        /// Initializes any queued objects for Awake & Start
        /// </summary>
        private static void Initialize()
        {
            if (s_toInitialize.Count == 0)
                return;

            s_looping = true;
#if ORDERED_GENERICS
            //Awake
            foreach (Wrapper wrapper in s_toInitialize)
            {
                try
                {
                    wrapper.obj.Awake();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            //Start
            foreach (Wrapper wrapper in s_toInitialize)
            {
                try
                {
                    wrapper.obj.Start();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                //Store as its initialized
                Add(wrapper, s_generics);
            }
#else
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
#endif
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
#if ORDERED_GENERICS
                Remove(e, s_generics);
#else
                s_generics.Remove(e);
#endif
            }
        }

        private static bool Contains(IUnityEvents obj)
        {
#if ORDERED_GENERICS
            foreach (var e in s_generics)
                if (e.Equals(obj))
                    return true;

            return false;
#else
            return s_generics.Contains(obj);
#endif
        }
#if ORDERED_GENERICS
        private static void Add(Wrapper w, List<Wrapper> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (w.executionOrder >= list[i].executionOrder)
                {
                    list.Insert(i, w);
                    break;
                }
            //Probably first or last item
            list.Add(w);
        }

        private static void Remove(IUnityEvents w, List<Wrapper> list)
        {
            list.RemoveAll((wrapper) => wrapper.Equals(w));
        }
#endif

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
#if ORDERED_GENERICS
            foreach (Wrapper w in s_generics)
            {
                try
                {
                    w.obj.Update();
#else
            foreach (IUnityEvents obj in s_generics)
            {
                try
                {
                    obj.Update();
#endif
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
#if ORDERED_GENERICS
            foreach (Wrapper w in s_generics)
            {
                try
                {
                    w.obj.FixedUpdate();
#else
            foreach (IUnityEvents obj in s_generics)
            {
                try
                {
                    obj.FixedUpdate();
#endif
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
#if ORDERED_GENERICS
            foreach (Wrapper w in s_generics)
            {
                try
                {
                    w.obj.LateUpdate();
#else
            foreach (IUnityEvents obj in s_generics)
            {
                try
                {
                    obj.LateUpdate();
#endif
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

#if ORDERED_GENERICS
        private class Wrapper
        {
            public int executionOrder = 0;
            public IUnityEvents obj = null;

            public Wrapper(IUnityEvents i, int order)
            {
                obj = i;
                executionOrder = order;
            }

            public bool Equals(Wrapper other) => obj == other.obj;

            public bool Equals(IUnityEvents other) => obj == other;
        }
#endif
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