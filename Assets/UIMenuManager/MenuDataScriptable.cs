﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManager
{
    /// <summary>
    /// Stores information about a menu
    /// </summary>
    [CreateAssetMenu(fileName = "MenuData", menuName = "Menu/Data", order = 0)]
    public class MenuDataScriptable : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// The data used to store important editor information
        /// </summary>
        [SerializeField] Editor.MenuEditorData m_editorData = null;
#endif
        /// <summary>
        /// Data used to generate this menu
        /// </summary>
        [SerializeField] MenuData[] m_menuData = null;
        /// <summary>
        /// Dictionary containing the ids of every menu & submenu for easy access
        /// </summary>
        private readonly Dictionary<int, Menu> m_menuIdDictioanry = new Dictionary<int, Menu>();
        /// <summary>
        /// Dictionary containing the names of every menu & submenu for easy access
        /// </summary>
        private readonly Dictionary<string, Menu> m_menuNameDictioanry = new Dictionary<string, Menu>();
        /// <summary>
        /// The default menu.
        /// </summary>
        private Menu m_defaultMenu = null;
        /// <summary>
        /// The menu that is currently open.
        /// </summary>
        private Menu m_currentMenu = null;
        /// <summary>
        /// The previously open menu.
        /// </summary>
        private Stack<Menu> m_previousMenu = null;
        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize()
        {   //Already initialized
            if (m_previousMenu != null)
                return;

            CreateMenuObjects();
            //Open the current menu
            m_previousMenu = new Stack<Menu>();
            m_currentMenu = m_defaultMenu;
            m_currentMenu.Open();
        }
        /// <summary>
        /// Generate the Menu Objects
        /// </summary>
        private void CreateMenuObjects()
        {   //Generate the menu objects
            foreach (var data in m_menuData)
            {
                Menu menu = new Menu(data.m_menuName, data.m_uniqueID);
                m_menuIdDictioanry.Add(data.m_uniqueID, menu);
                m_menuNameDictioanry.Add(data.m_menuName, menu);
                //Set default
                if (data.m_isDefault)
                    m_defaultMenu = m_menuIdDictioanry[data.m_uniqueID];
            }

            //Initialize data once all menus are generated
            foreach (var data in m_menuData)
            {   //Spawn the menu
                Menu menu = m_menuIdDictioanry[data.m_uniqueID];
                menu.GenerateFromData(data, m_menuIdDictioanry);
            }

#if !UNITY_EDITOR
            //If not in editor, clear data to save memory
            m_menuData = null;
#endif
        }
        /// <summary>
        /// Attempt to open a menu using an action
        /// </summary>
        /// <param name="action"></param>
        public void RunAction(MenuCommonActions action)
        {
            Menu menuToOpen;
            switch (action)
            {
                case MenuCommonActions.Exit:
                    menuToOpen = m_defaultMenu;
                    m_previousMenu.Clear();
                    break;
                case MenuCommonActions.Return:
                    menuToOpen = m_previousMenu.Pop();
                    break;
                default:
                    menuToOpen = m_currentMenu.ExecuteKey((int)action);
                    break;
            }
            //No menu, don't open but log an exception
            if (menuToOpen == null)
            {
                Debug.LogException(new KeyNotFoundException("Menu: " + m_currentMenu.menuName + " does not contain action for " + action.ToString()));
                return;
            }
            //Already open so don't re-open
            if (menuToOpen == m_currentMenu)
                return;
            //If the menu is the previous menu, remove from stack
            if (menuToOpen == m_previousMenu.Peek())
                m_previousMenu.Pop();
            //Attempt to open the menu & close the previous menu
            m_currentMenu.Close();
            m_previousMenu.Push(m_currentMenu);
            m_currentMenu = menuToOpen;
            menuToOpen.Open();
        }
        /// <summary>
        /// Attempt to open a menu using an action
        /// </summary>
        /// <param name="action"></param>
        public void RunAction(int action)
        {
            RunAction((MenuCommonActions)action);
        }
        /// <summary>
        /// Get Menu or Submenu via id
        /// </summary>
        /// <param name="menuName"></param>
        /// <returns></returns>
        public Menu GetMenu(int uniqueID) => m_menuIdDictioanry[uniqueID];
        /// <summary>
        /// Get Menu or Submenu via id
        /// </summary>
        /// <param name="menuName"></param>
        /// <returns></returns>
        public Menu this[int uniqueID]
        {
            get => m_menuIdDictioanry[uniqueID];
        }
        /// <summary>
        /// Get Menu or Submenu via name
        /// </summary>
        /// <param name="menuName"></param>
        /// <returns></returns>
        public Menu GetMenu(string menuName) => m_menuNameDictioanry[menuName];
        /// <summary>
        /// Get Menu or Submenu via name
        /// </summary>
        /// <param name="menuName"></param>
        /// <returns></returns>
        public Menu this[string menuName]
        {
            get => m_menuNameDictioanry[menuName];
        }
        /// <summary>
        /// Class storing menu
        /// </summary>
        public class Menu
        {
            /// <summary>
            /// Name/Key of the menu
            /// </summary>
            /// <remarks>This isn't necessary amd not recommended to be used as a way of referencing menus as its too volitile.</remarks>
            public readonly string menuName = null;
            /// <summary>
            /// Unique ID of this menu
            /// </summary>
            public readonly int m_uniqueId = int.MinValue;
#region OpenClose
            /// <summary>
            /// Is this menu currently open. (for call optimization)
            /// </summary>
            private bool m_isOpen = false;
            /// <summary>
            /// Events to invoke when opened
            /// </summary>
            /// <remarks>This is a list too ensure 1 error doesn't break everything else</remarks>
            private readonly List<Action> m_onOpen = new List<Action>();
            /// <summary>
            /// Invoked when opening this menu or submenu
            /// </summary>
            public event Action OnOpen
            {
                add
                {
                    m_onOpen.Add(value);
                    //late subscription catch to ensure the menus are in the correct state
                    if (m_isOpen)
                        value?.Invoke();
                }
                remove
                {
                    m_onOpen.Remove(value);
                }
            }
            /// <summary>
            /// Events to invoke when closed
            /// </summary>
            /// <remarks>This is a list too ensure 1 error doesn't break everything else</remarks>
            private readonly List<Action> m_onClose = new List<Action>();
            /// <summary>
            /// Invoked when closing this menu or submenu
            /// </summary>
            public event Action OnClose
            {
                add
                {
                    m_onClose.Add(value);
                    //late subscription catch to ensure the menus are in the correct state
                    if (!m_isOpen)
                        value?.Invoke();
                }
                remove
                {
                    m_onClose.Remove(value);
                }
            }
            /// <summary>
            /// Open this menu
            /// </summary>
            internal void Open()
            {
                OpenParentMenu();
                //If open, don't re-open
                if (m_isOpen)
                    return;
                m_isOpen = true;

                foreach (var @event in m_onOpen)
                    @event.SafeInvoke();
            }
            /// <summary>
            /// Close this menu
            /// </summary>
            internal void Close()
            {
                CloseParentMenu();
                //If closed, don't re-close
                if (!m_isOpen)
                    return;
                m_isOpen = false;

                foreach (var @event in m_onClose)
                    @event.SafeInvoke();
            }
            /// <summary>
            /// If this is a sub menu, open the parent menu
            /// </summary>
            private void OpenParentMenu()
            {
                m_parentMenu?.Open();
            }
            /// <summary>
            /// If this is a sub menu, close the parent menu
            /// </summary>
            private void CloseParentMenu()
            {
                m_parentMenu?.Close();
            }
#endregion

#region MenuConnections
            /// <summary>
            /// Parent menu for sub menus
            /// </summary>
            private Menu m_parentMenu = null;
            /// <summary>
            /// The sub menus this menu contains
            /// </summary>
            private readonly Dictionary<int, Menu> m_subMenus = new Dictionary<int, Menu>();
            /// <summary>
            /// Connections to other menus
            /// </summary>
            private readonly Dictionary<int, Menu> m_connections = new Dictionary<int, Menu>();
            /// <summary>
            /// Get the Menu that should be opened from a given key
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            internal Menu ExecuteKey(int key)
            {   //Check if sub menu
                Menu menu;
                if (m_subMenus.TryGetValue(key, out menu))
                    return menu;
                //Try to get connecting menu instead
                m_connections.TryGetValue(key, out menu);
                return menu;
            }
            #endregion
            /// <summary>
            /// Generate a basic menu
            /// </summary>
            /// <param name="name"></param>
            /// <param name="uniqueID"></param>
            internal Menu(string name, int uniqueID)
            {
                menuName = name;
                m_uniqueId = uniqueID;
            }
            /// <summary>
            /// Fill in the rest of the menu data
            /// </summary>
            /// <param name="data"></param>
            /// <param name="allMenus"></param>
            internal void GenerateFromData(MenuData data, Dictionary<int, Menu> allMenus)
            {   //Generate connections
                foreach (var connection in data.m_connections)
                    m_connections.Add(connection.key, allMenus[connection.m_uniqueID]);
                //Generate sub menus
                for (int i = 0; i < data.m_subMenuIds.Length; i++)
                {
                    Menu subMenu = allMenus[data.m_subMenuIds[i]];
                    subMenu.m_parentMenu = this;
                    m_subMenus.Add(i, subMenu);
                }
            }
        }
        /// <summary>
        /// Simpler class for generating a Menu
        /// </summary>
        [Serializable]
        public struct MenuData
        {
            /// <summary>
            /// Is this a default menu
            /// </summary>
            public bool m_isDefault;
            /// <summary>
            /// Name of this menu
            /// </summary>
            /// <remarks>This isn't necessary amd not recommended to be used as a way of referencing menus as its too volitile.</remarks>
            public string m_menuName;
            /// <summary>
            /// Unique ID of the menu
            /// </summary>
            public int m_uniqueID;
            /// <summary>
            /// Name of sub menus
            /// </summary>
            public int[] m_subMenuIds;
            /// <summary>
            /// The connections to other menus
            /// </summary>
            public ConnectionData[] m_connections;
            /// <summary>
            /// Data about a connection
            /// </summary>
            [Serializable]
            public struct ConnectionData
            {   /// <summary>
                /// The key that will result in this menu being enabled
                /// </summary>
                public int key;
                /// <summary>
                /// The name of the connecting menu
                /// </summary>
                public int m_uniqueID;
            }
        }
    }
    /// <summary>
    /// Common actions used by a menu
    /// </summary>
    public enum MenuCommonActions
    {
        Return = -2, //Return to previous menu
        Exit = -1,   //Return the default menu
    }
}