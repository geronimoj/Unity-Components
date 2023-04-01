using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
{
    /// <summary>
    /// Abstract class that manages all things loading bar related. You only need to define how to display it
    /// </summary>
    /// <remarks>The loading bar turns itself off to escape the update loop for refresh. If you need the update loop
    /// for your own UI refreshs, override FlagForRefresh to apply your own refresh logic</remarks>
    [DefaultExecutionOrder(int.MaxValue)] // Always run last to ensure everyone gets a change to update loading progress
    public abstract class LoadingBar : MonoBehaviour
    {
        protected virtual void Awake()
        {
            LoadingBarManager._onTaskUpdate += FlagForRefresh;
            enabled = false;
        }

        protected virtual void OnDestroy()
        {
            LoadingBarManager._onTaskUpdate -= FlagForRefresh;
        }
        /// <summary>
        /// Flag the loading bar to refresh its UI
        /// </summary>
        protected virtual void FlagForRefresh()
        {   // Turn this object on to refresh. Saves having to create coroutines or run in update all the time.
            // Also ensures that if multiple tasks update, we don't refresh for each one, but once all are updated.
            enabled = true;
        }
        /// <summary>
        /// Update loading bar as last thing to do in loading cycle
        /// </summary>
        protected virtual void LateUpdate()
        {   // Refresh loading bar, displaying the highest priority task group
            var info = LoadingBarManager.GetLoadingBarInfo(LoadingBarManager.GetHighestTaskGroup());
            UpdateLoadingBar(info.Item1, info.Item2);
            enabled = false;
        }
        /// <summary>
        /// Apply changes to loading bar.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="percentComplete"></param>
        /// <remarks>This is abstract so that you can do what you want with the packges you use. (No assumptions made)
        /// Just in case you are using NGUI, TMPro or something else</remarks>
        protected abstract void UpdateLoadingBar(string text, float percentComplete);
    }
}