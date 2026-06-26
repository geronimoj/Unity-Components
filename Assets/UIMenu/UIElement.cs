using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.UI
{
    /// <summary>
    /// A togglable element of the UI
    /// </summary>
    /// <remarks>
    /// The base level of UI elements
    /// </remarks>
    public class UIElement : MonoBehaviour
    {
        /// <summary>
        /// Should this UIElement toggle itself or child objects when turning on or off
        /// </summary>
        [SerializeField] bool toggleSelf = true;

        /// <summary>
        /// Any child UI Elements this element is dependent on
        /// </summary>
        public UIElement[] childElements;

        /// <summary>
        /// The current state of the UIElement
        /// </summary>
        protected UIState state = UIState.Undefined;

        /// <summary>
        /// Is the UIElement showing
        /// </summary>
        public bool Showing => state == UIState.Show;
        /// <summary>
        /// The current state of the UIElement
        /// </summary>
        public UIState State => state;

        /// <summary>
        /// Gets all elements this menu is dependent on
        /// </summary>
        /// <param name="elements"></param>
        public void GetDependentElements(HashSet<UIElement> elements)
        {
            // Add ourself to the elements. If we are already there, don't bother adding children as we have
            // already been parsed.
            if (elements.Add(this))
                foreach (var child in childElements)
                {
                    // If null, skip this element. In editor report a log to hopefully get it cleaned up.
                    if (!child)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"[VFlame.UI] {name} contains a null element!", this);
#endif
                        continue;
                    }

                    child.GetDependentElements(elements);
                }
        }

        /// <summary>
        /// Show the UIElement
        /// </summary>
        /// <returns></returns>
        public void Show() => Show(false);
        /// <summary>
        /// Show the UIElement
        /// </summary>
        /// <returns></returns>
        public virtual void Show(bool instant) => UIManager.Show(this, instant);

        /// <summary>
        /// Hide the UIElement
        /// </summary>
        /// <returns></returns>
        public void Hide() => Hide(false);
        /// <summary>
        /// Hide the UIElement
        /// </summary>
        /// <returns></returns>
        public virtual void Hide(bool instant) => UIManager.Hide(this, instant);

        /// <summary>
        /// Toggle the UIElement to a specific state
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        public virtual IEnumerator Toggle(HashSet<UIElement> toIgnore, UIState state, bool instant)
        {   
            // Check if this element should not change state. This is used for when changing between menus, popups or views
            // to avoid transitioning UIElements that will need to remain on screen anyways.
            if (this.state == state || (toIgnore?.Contains(this) ?? false))
                yield break;

            // Store the current state
            this.state = state;
            bool show = state == UIState.Show;

            // If we are showing, execute the OnShow() event with a Try Catch to avoid breaking UI if it errors.
            if (show)
            {
                try
                {
                    OnShown();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // Check how we should manage toggling ourself.
            if (toggleSelf)
                gameObject.SetActive(show);
            else
            {
                // Toggle all children if we are not toggling this object.
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(show);
            }

            // If there are child elements to toggle, toggle them
            if (childElements.Length > 0)
            {
                // Borrow a dictionary from the pool to mitigate GC alloc
                PoolList<IEnumerator> enumerators = PoolList<IEnumerator>.Get(childElements.Length);

                // Toggle all child elements
                foreach (var child in childElements)
                {
                    // If null, skip this element. In editor report a log to hopefully get it cleaned up.
                    if (!child)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"[VFlame.UI] {name} contains a null element!", this);
#endif
                        continue;
                    }

                    var enumerator = child.Toggle(toIgnore, state, instant);

                    // If we get an enumerator, track it
                    if (enumerator != null)
                        enumerators.Add(enumerator);
                }

                // wait for all enumerators to finish
                while (enumerators.Count > 0)
                {
                    // Wait 1 frame before checking them. They returned us a valid coroutine so they must be doing something
                    yield return null;

                    // Check if all enumerators have finished, or if we need to keep waiting
                    for (int i = 0; i < enumerators.Count; i++)
                        if (enumerators[i].MoveNext() == false)
                        {
                            enumerators.RemoveAt(i);
                            i--;
                        }
                }

                // Release the list now that we have finished
                enumerators.Release();
            }

            // If we are hiding, execute OnHide in a try catch to avoid exceptions breaking the core system
            if (!show)
            {
                try
                {
                    OnHidden();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Executed when the UIElement starts a Show() operation (the object may not be enabled yet)
        /// </summary>
        protected virtual void OnShown() { }

        /// <summary>
        /// Executed when the UIElement finishes a Hide() operation (the object may be disabled)
        /// </summary>
        protected virtual void OnHidden() { }
    }
}