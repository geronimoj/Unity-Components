using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.UI
{
    /// <summary>
    /// A UIView
    /// </summary>
    /// <remarks>
    /// A UIView is the root level of UI that will be returned to when exiting all popups or menus.
    /// </remarks>
    public class UIView : UIElement
    {
        /// <summary>
        /// An empty UIView with no elements
        /// </summary>
        public static UIView EmptyView => null;
    }
}