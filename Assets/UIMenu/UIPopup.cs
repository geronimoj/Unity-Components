using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFlame.UI
{
    /// <summary>
    /// A UIPopup
    /// </summary>
    /// <remarks>
    /// UIPopups can be opened without closing UIMenus or hiding UIViews.
    /// </remarks>
    public class UIPopup : UIElement
    {
        /// <summary>
        /// Should this UIPopup be included in the menu stack.
        /// </summary>
        public bool isMenu = false;
    }
}