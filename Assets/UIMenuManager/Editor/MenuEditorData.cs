using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManager.Editor
{
    /// <summary>
    /// Stores important information for Editor utilities
    /// </summary>
    public class MenuEditorData : ScriptableObject
    {
        /// <summary>
        /// Info about the UID to name for events
        /// </summary>
        /// <remarks>For ensuring Menus do not lose connections</remarks>
        public struct UIDToMenuName
        {
            public int uId;
            public string m_menuName;
        }
        /// <summary>
        /// Info about the UID to action for custom actions
        /// </summary>
        /// <remarks>For ensuring Actions do not lose connections</remarks>
        public struct UIDToAction
        {
            public int uID;
            public string m_actionName;
        }
    }
}