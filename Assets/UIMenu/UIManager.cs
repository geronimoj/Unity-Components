using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VFlame.Utility;

namespace VFlame.UI
{
    /// <summary>
    /// Manager that handles changing UI
    /// </summary>
    public static class UIManager
    {
        /// <summary>
        /// The current UI View
        /// </summary>
        static UIView currentView = UIView.EmptyView;
        /// <summary>
        /// The order of UIMenus & UIPopups are layered
        /// </summary>
        static Stack<UIElement> historyStack = null;

        /// <summary>
        /// The current view being displayed
        /// </summary>
        public static UIView CurrentView => currentView;
        /// <summary>
        /// The current menu being displayed.
        /// </summary>
        public static UIMenu CurrentMenu
        {
            get
            {
                // Search through the history stack until we get a UIMenu.
                if (historyStack != null)
                    foreach (var element in historyStack)
                    {
                        if (element is UIMenu menu)
                            return menu;
                    }

                return null;
            }
        }
        /// <summary>
        /// The current popup being displayed.
        /// </summary>
        public static UIPopup CurrentPopup
        {
            get
            {
                if (historyStack?.TryPeek(out var popup) ?? false)
                    return popup as UIPopup;

                return null;
            }
        }

        /// <summary>
        /// Is the UI currently transitioning
        /// </summary>
        public static bool IsTransitioning { get; private set; } = false;

        public static void Set(UIView view, bool instant = false)
        {
            // Nothing to change
            if (view == currentView)
                return;

            // If there is no current menu over the view, we need to change to run UI changing logic.
            if (CurrentMenu == null)
            {
                // If there is only a view visible, just show the new view
                if (CurrentPopup == null)
                    ClearHistoryThenShow_Internal(view, null, instant);
                else
                {
                    // Ok, so we have a view visible, with UIPopups layered on top of it.
                    // To change the view, we need to know which UIElements that are currently visible are used by UIPopups
                    // and which are used by the View
                    ReplaceHideAndShow_Internal(currentView, view, instant);
                }
            }
            else if (historyStack != null)
            {
                // Something else is visible, we can just pop the history stack into a temporary list, remove the old view and subsitute our own
                PoolList<UIElement> stackElements = PoolList<UIElement>.Get(historyStack.Count);

                // Clear to the view
                while (historyStack.Count > 1)
                {
                    stackElements.Add(historyStack.Pop());
                }

                // Pop off the view
                historyStack.Pop();
                historyStack.Push(view);

                // Put the old stuff back
                stackElements.Reverse();
                foreach (var element in stackElements)
                    historyStack.Push(element);

                stackElements.Release();
            }
            else
            {
                // Put the new view onto the stack
                historyStack ??= new Stack<UIElement>(10);
                historyStack.Push(view);
            }

            currentView = view;
        }

        public static void Show(UIView view, bool instant = false)
        {
            // Nothing to change
            if (view == currentView)
                return;

            // Clear the history to the root view and show it
            ClearHistoryThenShow_Internal(view, null, instant);
            currentView = view;

            // Push this to be the last element in the history stack.
            historyStack ??= new Stack<UIElement>(10);
            historyStack.Push(view);
        }

        public static void Show(UIMenu menu, bool instant = false)
        {
            // If this menu was already shown, clear the stack until we reach that menu again.
            if (historyStack?.Contains(menu) ?? false)
            {
                ClearHistoryThenShow_Internal(menu, menu, instant);
            }
            else
            {
                // Track down the last non-menu popup that was shown in the history stack
                // We want to hide all non-menu popups from being displayed (or until we reach a menu root)
                UIElement lastPopup = null;
                if (historyStack != null)
                    foreach (var element in historyStack)
                    {
                        if (element is not UIPopup curPop || curPop.isMenu)
                            break;

                        lastPopup = element;
                    }

                // Clear the stack up to that popup, so that the last element in the historyStack is either a UIMenu or isMenu UIPopup
                ClearHistoryThenShow_Internal(menu, lastPopup, instant);
            }
            historyStack ??= new Stack<UIElement>(10);
            historyStack.Push(menu);
        }

        public static void Show(UIPopup popup, bool instant = false)
        {
            // If on the stack, clear to that element
            if (historyStack?.Contains(popup) ?? false)
                ClearHistoryThenShow_Internal(popup, popup, instant);
            else
            {
                // If completely new, just show it.
                Show_Internal(popup, instant);
            }

            historyStack ??= new Stack<UIElement>(10);
            historyStack.Push(popup);
        }

        public static void Show(UIElement element, bool instant = false)
        {
            // Handle each view type, (some have special handling)
            if (element is UIView view)
            {
                Show(view, instant);
                return;
            }

            if (element is UIMenu menu)
            {
                Show(menu, instant);
                return;
            }

            if (element is UIPopup popup)
            {
                Show(popup, instant);
                return;
            }

            // Execute normal show logic
            Show_Internal(element, instant);
        }

        public static void Hide(UIView view, bool instant = false)
        {
            if (view == currentView)
                Show(UIView.EmptyView, instant);
            else
            {
                Debug.LogError($"[VFlame.UI] Executing Hide on a non-current UIView - {(view ? view.name : "EmptyView")}!");
                Hide_Internal(view, instant);
            }
        }

        public static void Hide(UIMenu menu, bool instant = false)
        {
            // If this is the current menu, clear the stack down to the item after it.
            if (CurrentMenu == menu)
            {
                bool nextItem = false;
                UIElement endElement = null;
                if (historyStack != null)
                    foreach (var element in historyStack)
                    {
                        // It's the element we want to stop at?
                        if (nextItem)
                        {
                            endElement = element;
                            break;
                        }

                        // Set a flag so that we know to get the next element
                        if (element == menu)
                        {
                            nextItem = true;
                            continue;
                        }
                    }

                // If there are no more elements to hide after this one, show the currentView instead
                if (endElement == null)
                    endElement = currentView;

                // Execute ClearHistoryThenShow on the next final element
                ClearHistoryThenShow_Internal(endElement, endElement, instant);
                return;
            }
            else if (historyStack?.Contains(menu) ?? false)
            {
                // The menu is in the stack but not visible, remove it from the stack.
                Remove_Internal(menu);
            }
            else
            {
                // Debug.LogError($"[VFlame.UI] Executing Hide on an already hidden UIMenu - {(menu ? menu.name : "NULL")}!"); <-- this is stupid
                Hide_Internal(menu, instant);
            }
        }

        public static void Hide(UIPopup popup, bool instant = false)
        {
            // If this is at the top of the stack, hide it
            if (historyStack != null && 
                historyStack.Count > 0 && 
                historyStack.Peek() == popup)
            {
                // Hide the UIPopup
                Hide_Internal(popup, instant);
                historyStack.Pop();
            }
            else if (historyStack?.Contains(popup) ?? false)
            {
                // Check if the UIPopup is currently visible in the stack.
                bool isHidden = false;
                foreach(var element in historyStack)
                {
                    if (element == popup)
                        break;

                    // If we hit a UIMenu before the popup, it must be behind the UIMenu & thus not visible.
                    if (element is UIMenu)
                    {
                        isHidden = true;
                        break;
                    }
                }

                if (isHidden)
                {
                    // Not visible, so just remove it from the stack, so that as we back track later, the element isn't shown
                    Remove_Internal(popup);
                }
                else
                {
                    // It's currently visible but it's not the root popup, hide it and remove it from the stack
                    RemoveAndHide_Internal(popup, instant);
                }
            }
            else
            {
                // Just run hide, it shouldn't be visible anyways.
                Hide_Internal(popup, instant);
            }
        }

        public static void Hide(UIElement element, bool instant = false)
        {
            // Handle each view type, some have unique extra handling.
            if (element is UIView view)
            {
                Hide(view, instant);
                return;
            }

            if (element is UIMenu menu)
            {
                Hide(menu, instant);
                return;
            }

            if (element is UIPopup popup)
            {
                Hide(popup, instant);
                return;
            }

            Hide_Internal(element, instant);
        }

        /// <summary>
        /// Shows a UIElement
        /// </summary>
        /// <param name="element"></param>
        static void Show_Internal(UIElement element, bool instant)
        {
            // Null element. Ignore (handling for EmptyView which is technically just a null)
            if (!element)
                return;

            // If there is a show operation, wait for it to complete and apply the transitioning flag to stop buttons from being clickable.
            var showOp = element.Toggle(null, UIState.Show, instant);
            if (showOp != null)
                EnumeratorObject.Instance.StartCoroutine(PerformOperation(showOp));
        }

        /// <summary>
        /// Hides a UIElement
        /// </summary>
        /// <param name="element"></param>
        static void Hide_Internal(UIElement element, bool instant)
        {
            // Null element. Ignore (handling for EmptyView which is technically just a null)
            if (!element)
                return;

            // If there is a hide operation, wait for it to complete and apply the transitioning flag to stop buttons from being clickable.
            var hideOp = element.Toggle(null, UIState.Hide, instant);
            if (hideOp != null)
                EnumeratorObject.Instance.StartCoroutine(PerformOperation(hideOp));
        }

        /// <summary>
        /// Removes an element from the history stack, regardless of placement. Does not Hide.
        /// </summary>
        /// <param name="uiElement"></param>
        static void Remove_Internal(UIElement uiElement)
        {
            // Start off by removing the top of the stack to get at the stack element
            PoolList<UIElement> stackElements = PoolList<UIElement>.Get(historyStack.Count);

            while (historyStack.Peek() != uiElement)
            {
                stackElements.Add(historyStack.Pop());
            }

            // Remove the menu from the stack
            historyStack.Pop();

            // Put the old stack elements back in the same order
            stackElements.Reverse();
            foreach (var element in stackElements)
                historyStack.Push(element);

            stackElements.Release();
        }

        /// <summary>
        /// Handles changing between 2 UIElements
        /// </summary>
        /// <param name="hideElement"></param>
        /// <param name="showElement"></param>
        static void Change_Internal(UIElement hideElement, UIElement showElement, bool instant)
        {
            // If there is nothing to hide, just call show. Mostly handing to make popup/menu stacks nicer to read
            if (!hideElement)
            {
                Show_Internal(showElement, instant);
                return;
            }

            // If there is nothing to show, just call hide. Mostly handing to make popup/menu stacks nicer to read
            if (!showElement)
            {
                Hide_Internal(hideElement, instant);
                return;
            }

            // Would love to use BufferHashSet, however we need to hand it to the Hide operations & cannot trust the buffer
            // won't get used while the operation is in progress
            PoolHashSet<UIElement> elementsToIgnore = PoolHashSet<UIElement>.Get(100);

            // Get the UI elements that we are going to try and show. We'll hand the output to the hideOperation so that it can avoid hiding elements we want to display.
            showElement.GetDependentElements(elementsToIgnore);

            // Begin the operations.
            var hideOp = hideElement.Toggle(elementsToIgnore, UIState.Hide, instant);
            var showOp = showElement.Toggle(null, UIState.Show, instant);

            // Don't need the list anymore
            elementsToIgnore.Release();

            // If there are operations in progress, wait for them to complete
            if (hideOp != null)
            {
                // I think it's slightly more performant to write the code like this. We can save a few operations by using the correct PerformOperation
                // function. & writing the If statement like this means at most 2 comparisons. If we did an if-elseif stack, we could potentially need to
                // perform 3+ comparisons at maximum.
                if (showOp != null)
                    EnumeratorObject.Instance.StartCoroutine(PerformOperations(showOp, hideOp));
                else
                    EnumeratorObject.Instance.StartCoroutine(PerformOperation(hideOp));
            }
            else if (showOp != null)
            {
                EnumeratorObject.Instance.StartCoroutine(PerformOperation(showOp));
            }
        }

        static void ClearHistoryThenShow_Internal(UIElement showElement, UIElement historyEnd, bool instant)
        {
            PoolHashSet<UIElement> elementsToHide = PoolHashSet<UIElement>.Get(100);
            PoolHashSet<UIElement> elementsToShow = PoolHashSet<UIElement>.Get(100);
            PoolList<IEnumerator> enumerators = PoolList<IEnumerator>.Get(elementsToHide.Count);

            // Cut down through the history stack until we either reach the desired element or hit the bottom
            if (historyStack != null)
                while (historyStack.TryPop(out var element) && element != historyEnd)
                {
                    // If the element is the element we want to show, don't bother running Hide() on it.
                    if (element != showElement)
                        elementsToHide.Add(element);
                }

            // Obtain the elements to display & show them
            if (showElement)
            {
                showElement.GetDependentElements(elementsToShow);

                var showOp = showElement.Toggle(null, UIState.Show, instant);
                if (showOp != null)
                    enumerators.Add(showOp);
            }

            // Execute hide on hide elements
            foreach (var element in elementsToHide)
            {
                var hideOp = element.Toggle(elementsToShow, UIState.Hide, instant);
                if (hideOp != null)
                    enumerators.Add(hideOp);
            }

            // Don't need the pool lists anymore
            elementsToShow.Release();
            elementsToHide.Release();

            // If there are operations still in progress, wait for them to complete
            if (enumerators.Count > 0)
                EnumeratorObject.Instance.StartCoroutine(PerformOperations(enumerators));
            else
                enumerators.Release();

        }

        static void RemoveAndHide_Internal(UIElement elementToHide, bool instant)
        {
            PoolHashSet<UIElement> elementsToHide = PoolHashSet<UIElement>.Get(100);
            PoolHashSet<UIElement> elementsToShow = PoolHashSet<UIElement>.Get(100);
            PoolList<IEnumerator> enumerators = PoolList<IEnumerator>.Get(elementsToHide.Count);

            // Obtain the elements that we want to hide
            elementToHide.GetDependentElements(elementsToHide);

            // Build up a list of the elements that we want visible
            foreach(var element in historyStack)
            {
                element.GetDependentElements(elementsToShow);

                // We have reached the desired element to remove
                if (element == elementToHide)
                    break;
            }

            // Remove the elements we want visible from the toHide list
            foreach (var element in elementsToShow)
                elementsToHide.Remove(element);

            // Hide all the elements
            foreach(var element in elementsToHide)
            {
                var hideOp = element.Toggle(elementsToShow, UIState.Hide, instant);
                if (hideOp != null)
                    enumerators.Add(hideOp);
            }

            // Release the pooled dictionaries
            elementsToShow.Release();
            elementsToHide.Release();

            // Remove the element from the history stack
            Remove_Internal(elementToHide);

            // Start a coroutine on any hide or show operations
            if (enumerators.Count > 0)
                EnumeratorObject.Instance.StartCoroutine(PerformOperations(enumerators));
            else
                enumerators.Release();
        }

        static void ReplaceHideAndShow_Internal(UIElement oldElement, UIElement newElement, bool instant)
        {
            PoolHashSet<UIElement> elementsToHide = PoolHashSet<UIElement>.Get(100);
            PoolHashSet<UIElement> elementsToShow = PoolHashSet<UIElement>.Get(100);
            PoolList<IEnumerator> enumerators = PoolList<IEnumerator>.Get(elementsToHide.Count);
            PoolList<UIElement> stackHistory = PoolList<UIElement>.Get(historyStack.Count);

            // Obtain the elements that we want to hide
            oldElement.GetDependentElements(elementsToHide);
            newElement.GetDependentElements(elementsToShow);

            // Build up a list of the elements that we want visible
            while (historyStack.Peek() != oldElement)
            {
                var element = historyStack.Pop();
                element.GetDependentElements(elementsToShow);

                stackHistory.Add(element);
            }

            // Replace the old element
            historyStack.Pop();
            historyStack.Push(newElement);

            // Put the history back
            stackHistory.Reverse();
            foreach (var element in stackHistory)
                historyStack.Push(element);

            // Remove the elements we want visible from the toHide list
            foreach (var element in elementsToShow)
                elementsToHide.Remove(element);

            // Show all the elements
            foreach(var element in elementsToShow)
            {
                var showOp = element.Toggle(null, UIState.Show, instant);
                if (showOp != null)
                    enumerators.Add(showOp);
            }

            // Hide all the elements
            foreach (var element in elementsToHide)
            {
                var hideOp = element.Toggle(elementsToShow, UIState.Hide, instant);
                if (hideOp != null)
                    enumerators.Add(hideOp);
            }

            // Release the pooled dictionaries
            elementsToShow.Release();
            elementsToHide.Release();
            stackHistory.Release();

            // Start a coroutine on any hide or show operations
            if (enumerators.Count > 0)
                EnumeratorObject.Instance.StartCoroutine(PerformOperations(enumerators));
            else
                enumerators.Release();
        }

        static IEnumerator PerformOperation(IEnumerator operation)
        {
            // Flag UI as transitioning while Show() is in progress to stop buttons from being clickable
            IsTransitioning = true;

            // Perform the transition. If there is no operation, don't bother yielding anything.
            if (operation != null)
                yield return operation;

            // Allow buttons to be clickable again
            IsTransitioning = false;
        }

        static IEnumerator PerformOperations(IEnumerator opA, IEnumerator opB)
        {
            // Flag UI as transitioning while Show() is in progress to stop buttons from being clickable
            IsTransitioning = true;

            // Wait for all valid operations to complete.
            while (opA != null && opB != null)
            {
                // At least 1 of the operations isnt null. So it was started this frame, wait 1 frame before checking them.
                yield return null;

                // Progress/Test each operation
                if (opA != null && opA.MoveNext() == false) opA = null;
                if (opB != null && opB.MoveNext() == false) opB = null;
            }

            // Allow buttons to be clickable again
            IsTransitioning = false;
        }

        static IEnumerator PerformOperations(PoolList<IEnumerator> enumerators)
        {
            IsTransitioning = true;

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

            IsTransitioning = false;
            enumerators.Release();
        }

        /// <summary>
        /// Closes all UI and clears and null UI elements from storage (for GC collection)
        /// </summary>
        public static void Clear(bool instant = false)
        {
            Show(UIView.EmptyView, instant);
        }
        /// <summary>
        /// Changes to an Empty view.
        /// </summary>
        public static void ClearView(bool instant = false)
        {
            // Just execute a show of an empty view.
            Show(UIView.EmptyView, instant);
        }
        /// <summary>
        /// Closes & Clears the menu stack
        /// </summary>
        public static void ClearMenus(bool instant = false)
        {
            // No menus to open or close
            if (CurrentMenu == null)
                return;

            // Clear back to the current view if there are menus open
            ClearHistoryThenShow_Internal(currentView, null, instant);
        }
        /// <summary>
        /// Closes & Clears the popup stack
        /// </summary>
        public static void ClearPopups(bool instant = false)
        {
            // Clear back to the current menu
            UIElement targetElement = CurrentMenu;

            // If there is no current menu, clear back to the current view instead
            if (targetElement == null)
                targetElement = currentView;

            ClearHistoryThenShow_Internal(targetElement, targetElement, instant);
        }
    }

    /// <summary>
    /// Enum for tracking a UIElements state.
    /// </summary>
    /// <remarks>
    /// Resolves having to use bool? to track state. Bools & byte are the same size in memory so this is probably more memory performant than a bool (for the most part)
    /// </remarks>
    public enum UIState : byte
    {
        /// <summary>
        /// UIElement has not been explicitly hidden or shown. Effectively in an uninitialized state
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// UIElement is currently being shown
        /// </summary>
        Show = 1,
        /// <summary>
        /// UIElement is currently hidden
        /// </summary>
        Hide = 2,
    }
}