using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManager
{
    /// <summary>
    /// Initializes the menu
    /// </summary>
    [DefaultExecutionOrder(int.MaxValue)]
    public class MenuInitializer : MonoBehaviour
    {
        /// <summary>
        /// Setup the default menu
        /// </summary>
        [SerializeField] MenuScriptable _defaultMenu;

        void Start()
        {   // Open the default menu
            MenuScriptable.s_activeMenu = null;
            _defaultMenu.Open();
        }
    }
}