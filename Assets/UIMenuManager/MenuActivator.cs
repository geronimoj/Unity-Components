using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuManager
{
    /// <summary>
    /// Used to activate a specific menu
    /// </summary>
    public class MenuActivator : MonoBehaviour
    {
        /// <summary>
        /// The menu to activate
        /// </summary>
        [SerializeField] MenuScriptable _menuToActivate;

        private void Awake()
        {
            if (!_menuToActivate)
                Debug.LogError($"Menu Activator {name} has no assigned menu. May have been deleted during menu updates!", this);
        }
        /// <summary>
        /// Activate the given menu
        /// </summary>
        public void ActivateMenu()
        {
            if (_menuToActivate)
                _menuToActivate.Open();
        }
    }
}