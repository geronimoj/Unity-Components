using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManager
{

    public class MenuScriptable : ScriptableObject
    {
        /// <summary>
        /// Name of the menu
        /// </summary>
        [SerializeField] string _nameID = null;
        /// <summary>
        /// The parent of this menu. Primarily for subMenus
        /// </summary>
        [SerializeField] MenuScriptable _parentMenu = null;
        /// <summary>
        /// The sub menus this menu has.
        /// </summary>
        [SerializeField] MenuScriptable[] _subMenus = null;
        /// <summary>
        /// Any additional menus that should be enabled when activating this menu
        /// </summary>
        [SerializeField] MenuScriptable[] _additionalMenus = null;
        /// <summary>
        /// The currently active menu
        /// </summary>
        internal static MenuScriptable s_activeMenu = null;
        /// <summary>
        /// The current gameObjects that are part of this menu
        /// </summary>
        private List<GameObject> _menuObjects = null;
        /// <summary>
        /// Subscribe a gameObject to a menu. When the menu is opened or close, the object will be enabled & disabled
        /// </summary>
        /// <param name="menuRoot"></param>
        public void SubscribeMenu(GameObject menuRoot)
        {   // Cannot subscribe null
            if (!menuRoot)
                return;

            ClearNulls();

            if (_menuObjects == null)
                _menuObjects = new List<GameObject>();

            _menuObjects.Add(menuRoot);
        }
        /// <summary>
        /// Remove any null objects (mostly to clean up gameObjects from other scenes or previous sessions in editor)
        /// </summary>
        void ClearNulls()
        {   // Remove any null menu objects.
            if (_menuObjects != null)
                for (int i = 0; i < _menuObjects.Count;)
                {
                    if (!_menuObjects[i])
                    {
                        _menuObjects.RemoveAt(i);
                        continue;
                    }
                    i++;
                }
        }
        /// <summary>
        /// Pre-existing lists to avoid allocation of memory when opening a UI too badly
        /// </summary>
        private static readonly List<MenuScriptable> s_prevMenuTree = new List<MenuScriptable>(10);
        private static readonly List<MenuScriptable> s_curMenuTree = new List<MenuScriptable>(10);
        /// <summary>
        /// Open this menu
        /// </summary>
        public void Open()
        {
            var prevMenu = s_activeMenu;
            s_activeMenu = this;

            s_prevMenuTree.Clear();
            s_curMenuTree.Clear();
            //Get all menus
            if (prevMenu)
                prevMenu.GetMenuTree(s_prevMenuTree);

            GetMenuTree(s_curMenuTree);
            // Do prev menu first as, if prevMenu is null, its faster to open
            for (int i = 0; i < s_prevMenuTree.Count;)
            {
                var menu = s_prevMenuTree[i];
                //If both menus contain this menu, its already enabled so remove
                if (s_curMenuTree.Contains(menu))
                {
                    s_curMenuTree.Remove(menu);
                    s_prevMenuTree.RemoveAt(i);
                    continue;
                }
                // Not shared so continue
                i++;
            }
            // Turn off previous menus that are not part of the current menu
            foreach (var menu in s_prevMenuTree)
                menu.SetMenus(false);
            // Turn on new menus, currently active menus should have been removed from this list
            foreach (var menu in s_curMenuTree)
                menu.SetMenus(true);
        }
        /// <summary>
        /// Toggle the current menu's gameObjects on or off
        /// </summary>
        /// <param name="state"></param>
        void SetMenus(bool state)
        {
            foreach (var menu in _menuObjects)
                menu.SafeSetActive(state);
        }
        /// <summary>
        /// For gathering all menuScriptables that need to be enabled for this menu.
        /// </summary>
        /// <param name="menuTree"></param>
        void GetMenuTree(List<MenuScriptable> menuTree)
        {
            if (_parentMenu && !menuTree.Contains(this))
            {
                menuTree.Add(this);

                foreach (var additional in _additionalMenus)
                {   // Already been added so skip
                    if (menuTree.Contains(additional))
                        continue;
                    // Add and ask for other UI that needs to be enabled.
                    menuTree.Add(additional);
                    additional.GetMenuTree(menuTree);
                }
                // Get parent menu
                _parentMenu.GetMenuTree(menuTree);
            }
        }
    }
}