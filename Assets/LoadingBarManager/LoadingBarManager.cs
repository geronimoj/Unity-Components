using System;
using System.Linq;
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

        private static Dictionary<int, List<ILoadingTask>> _taskGroups = null;

        /// <summary>
        /// Invoked when a task updates
        /// </summary>
        internal static Action _onTaskUpdate = null;
        /// <summary>
        /// Get information about the current loading bar
        /// </summary>
        /// <returns>Returns the text to display and 0 - 1 progress</returns>
        public static (string, float) GetLoadingBarInfo(int taskQueue = 0)
        {
            string message = null;
            float progress = 0f;
            // Avoid errors
            if (_taskGroups != null && _taskGroups.Count > 0)
            {   // Make sure we are getting a taskQueue. Otherwise default to highest
                if (!_taskGroups.ContainsKey(taskQueue))
                    taskQueue = GetHighestTaskGroup();
                // Get the queue to use
                var allTasks = _taskGroups[taskQueue];

                int currentPriority = int.MinValue;
                ILoadingTask bestTask = null;

                foreach (var task in allTasks)
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
                // This shouldn't cause a divide by 0 error since the task groups should never have a size of 0.
                progress /= allTasks.Count;
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
        /// <param name="taskGroup">The group to put the task in. The progress of the highest value, incomplete, group is shown on the loading bar.</param>
        /// <returns>The loading task</returns>
        /// <remarks>Task Groups can be used to separate loading tasks. If your game has multiple loading bars displayed or you want to override the
        /// loading bar with a download bar, you can use taskGroups.</remarks>
        public static LoadingTask CreateLoadingTask(int priority, int taskGroup = 0)
        {
            LoadingTask task = new LoadingTask
            {
                _priority = priority
            };

            if (_taskGroups == null)
                _taskGroups = new Dictionary<int, List<ILoadingTask>>(1);
            // Get task group
            if (!_taskGroups.TryGetValue(taskGroup, out List<ILoadingTask> group))
            {
                group = new List<ILoadingTask>(10);
                _taskGroups[taskGroup] = group;
            }

            // Default to size 10
            if (_allTasks == null) // Note: When upgrading project. change to ??= because its less lines of code
                _allTasks = new List<ILoadingTask>(10);

            _allTasks.Add(task);
            group.Add(task);

            OnTaskUpdated();

            return task;
        }
        /// <summary>
        /// Create a loading task of special type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskGroup">The group to put the task in. The progress of the highest value, incomplete, group is shown on the loading bar.</param>
        /// <returns>A new task of the given type</returns>
        /// <remarks>For if you want to create LoadingTasks that write for themself, for systems such as SceneLoading, or Addressable
        /// asset loading and do not want/need another object to write to its Progress.
        /// 
        /// Task Groups can be used to separate loading tasks. If your game has multiple loading bars displayed or you want to override the
        /// loading bar with a download bar, you can use taskGroups.</remarks>
        public static T CreateLoadingTask<T>(int taskGroup = 0) where T : ILoadingTask
        {
            T task = Activator.CreateInstance<T>();

            if (_taskGroups == null)
                _taskGroups = new Dictionary<int, List<ILoadingTask>>(1);
            // Get task group
            if (!_taskGroups.TryGetValue(taskGroup, out List<ILoadingTask> group))
            {
                group = new List<ILoadingTask>(10);
                _taskGroups[taskGroup] = group;
            }
            // Default to size 10
            if (_allTasks == null) // Note: When upgrading project. change to ??= because its less lines of code
                _allTasks = new List<ILoadingTask>(10);

            _allTasks.Add(task);
            group.Add(task);

            OnTaskUpdated();

            return task;
        }
        /// <summary>
        /// Returns "if all loading tasks are completed"
        /// </summary>
        /// <returns></returns>
        public static bool LoadingIsComplete()
        {   // Check every task's completion state.
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
                task.Dispose();

            _taskGroups = null;
            _allTasks = null;

            // We could, theoretically hand LoadingTask as a struct, with an internal class
            // then, on an event fired here, have it null that class to reduce garbage caused
            // by people not releasing the loading task... Food for thought

            // ^ This won't work anymore since we upgraded to using Interfaces. If we remained
            // using a abstract class based system, it could still work.
        }
        /// <summary>
        /// Cleans up loading operations for a specific taskGroup to recover memory.
        /// </summary>
        public static void CleanUpLoading(int taskGroup)
        {
            if (!_taskGroups.TryGetValue(taskGroup, out List<ILoadingTask> tasks))
                return;

            foreach (var task in tasks)
                task.Dispose();

            _taskGroups.Remove(taskGroup);
        }
        /// <summary>
        /// Manually call when a task updates its progress or loading message to request a UI refresh
        /// </summary>
        public static void OnTaskUpdated()
        {
            _onTaskUpdate.SafeInvoke();
        }
        /// <summary>
        /// Gets the highest task group.
        /// </summary>
        /// <returns></returns>
        public static int GetHighestTaskGroup()
        {
            int best = int.MinValue;

            foreach (var key in _taskGroups.Keys)
                if (key > best)
                    best = key;

            return best;
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
        void Dispose();
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
                    LoadingBarManager.OnTaskUpdated();
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
                    LoadingBarManager.OnTaskUpdated();
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
        void ILoadingTask.Dispose() { }
    }
}