using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
{
    /// <summary>
    /// Manages loading progress
    /// </summary>
    public static class LoadingBarManager
    {   /// <summary>
        /// Default text to display if a Task has no display information
        /// </summary>
        private const string DEFAULT_TEXT = "Loading...";
        /// <summary>
        /// All the tasks currently active for this loading operation.
        /// </summary>
        private static List<ILoadingTask> _allTasks = null;
        /// <summary>
        /// Invoked when a task updates
        /// </summary>
        internal static Action _onTaskUpdate = null;
        /// <summary>
        /// Get information about the current loading bar
        /// </summary>
        /// <returns>Returns the text to display and 0 - 1 progress</returns>
        public static (string, float) GetLoadingBarInfo()
        {
            string message = null;
            float progress = 0f;
            // Check to avoid divide by 0 error later
            if (_allTasks != null && _allTasks.Count > 0)
            {
                int currentPriority = int.MinValue;
                ILoadingTask bestTask = null;
                foreach (var task in _allTasks)
                {   // Find the task with the highest priority
                    var prio = task.GetPriority();
                    if (prio > currentPriority)
                    {
                        bestTask = task;
                        currentPriority = prio;
                    }
                    // Sum progress, will divide later
                    progress += task.GetPercentComplete();
                }
                // This is the divide by 0 error we want to avoid
                progress /= _allTasks.Count;
                //Get message
                message = bestTask.GetLoadingMessage();
            }
            // Ensure there is a message
            if (message.IsNullOrWhiteSpace())
                message = DEFAULT_TEXT;

            return (message, progress);
        }
        /// <summary>
        /// Creates a simple loading task.
        /// </summary>
        /// <param name="priority">The loading priority. Defines which loading message to display. Higher priority are displayed first.</param>
        /// <returns>The loading task</returns>
        public static LoadingTask CreateLoadingTask(int priority)
        {
            LoadingTask task = new LoadingTask
            {
                _priority = priority
            };
            // Default to size 10
            if (_allTasks == null) // Note: When upgrading project. change to ??= because its less lines of code
                _allTasks = new List<ILoadingTask>(10);

            _allTasks.Add(task);

            TaskUpdated();

            return task;
        }
        /// <summary>
        /// Create a loading task of special type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="priority">The loading priority. Defines which loading message to display. Higher priority are displayed first.</param>
        /// <returns>A new task of the given type</returns>
        /// <remarks>For if you want to create LoadingTasks that write for themself, for systems such as SceneLoading, or Addressable
        /// asset loading and do not want/need another object to write to its Progress.</remarks>
        public static T CreateLoadingTask<T>() where T : ILoadingTask
        {
            T task = Activator.CreateInstance<T>();
            // Default to size 10
            if (_allTasks == null) // Note: When upgrading project. change to ??= because its less lines of code
                _allTasks = new List<ILoadingTask>(10);

            _allTasks.Add(task);

            TaskUpdated();

            return task;
        }
        /// <summary>
        /// Returns "if all loading tasks are completed"
        /// </summary>
        /// <returns></returns>
        public static bool LoadingIsComplete()
        {
            foreach (var task in _allTasks)
                if (!task.IsDone())
                    return false;

            return true;
        }
        /// <summary>
        /// Cleans up loading operations to recover memory.
        /// </summary>
        public static void CleanUpLoading()
        {   // Nothing more to load so clear
            foreach (var task in _allTasks)
                task.CleanUp();

            _allTasks = null;

            // We could, theoretically hand LoadingTask as a struct, with an internal class
            // then, on an event fired here, have it null that class to reduce garbage caused
            // by people not releasing the loading task... Food for thought
            
            // ^ This won't work anymore since we upgraded to using Interfaces. If we remained
            // using a abstract class based system, it could still work.
        }
        /// <summary>
        /// Runs the cleanup asyncronously.
        /// </summary>
        /// <returns>A task that is handling the cleanup</returns>
        /// <remarks>This will delay the reuse of the LoadingBarManager until the function completes.
        /// If you attempt to use LoadingBarManager before the clean up finishes, it may clean up your new loading bar tasks as well.</remarks>
        public static Task CleanUpLoadingAsync()
        {
            return Task.Run(CleanUpLoading);
        }

        public static void TaskUpdated()
        {
            _onTaskUpdate.SafeInvoke();
        }
    }
    /// <summary>
    /// Interface for loading tasks.
    /// </summary>
    /// <remarks> - Loading tasks should be created using the LoadingBarManage.CreateLoadingTask<T>()
    ///  - When a task updates progress, loading message or priority, LoadingBarManager.TaskUpdated should be called
    ///    to initiate a loading bar refresh</remarks>
    public interface ILoadingTask
    {
        /// <summary>
        /// Current progress towards completing the task (0 - 1)
        /// </summary>
        /// <returns>Current progress in 0 - 1 range</returns>
        float GetPercentComplete();
        /// <summary>
        /// The message to display on the loading bar
        /// </summary>
        /// <returns></returns>
        string GetLoadingMessage();
        /// <summary>
        /// The loading priority. Defines which loading message to display. Higher priority are displayed first.
        /// </summary>
        /// <returns></returns>
        int GetPriority();
        /// <summary>
        /// Is the task complete.
        /// </summary>
        /// <returns></returns>
        bool IsDone();
        /// <summary>
        /// The task is no-longer needed, clean it up.
        /// </summary>
        void CleanUp();
    }

    /// <summary>
    /// Stores information about a part of the loading operation
    /// </summary>
    public class LoadingTask : ILoadingTask
    {
        private float _percentComplete = 0f;
        /// <summary>
        /// Current percentage towards completing this task
        /// </summary>
        public float PercentComplete
        {
            get => _percentComplete;
            set
            {
                value = Mathf.Clamp01(value);

                if (value != _percentComplete)
                {
                    _percentComplete = value;
                    LoadingBarManager.TaskUpdated();
                }
            }
        }
        /// <summary>
        /// Has this task completed
        /// </summary>
        public bool IsDone => _percentComplete >= 1f;

        private string _loadingMessage = null;
        /// <summary>
        /// The message to display for this task
        /// </summary>
        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                if (value != _loadingMessage)
                {
                    _loadingMessage = value;
                    LoadingBarManager.TaskUpdated();
                }
            }
        }
        /// <summary>
        /// The loading priority. Defines which loading message to display. Higher priority are displayed first.
        /// </summary>
        internal int _priority = 0;

        /// <summary>
        /// Current percentage towards completing this task
        /// </summary>
        float ILoadingTask.GetPercentComplete() => _percentComplete;
        /// <summary>
        /// The message to display for this task
        /// </summary>
        string ILoadingTask.GetLoadingMessage() => _loadingMessage;
        /// <summary>
        /// The loading priority. Defines which loading message to display. Higher priority are displayed first.
        /// </summary>
        int ILoadingTask.GetPriority() => _priority;
        /// <summary>
        /// Has the task finished
        /// </summary>
        /// <returns></returns>
        bool ILoadingTask.IsDone() => IsDone;
        void ILoadingTask.CleanUp() {}
    }
}