using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MenuManager
{
    /// <summary>
    /// Used to subscribe a gameObject to a Menu
    /// </summary>
    public class MenuRoot : MonoBehaviour
    {
        /// <summary>
        /// The menu this object is related to
        /// </summary>
        [Tooltip("The menu this object is a part of")]
        [SerializeField] MenuScriptable _menu;

        private void Awake()
        {
            if (_menu)
                _menu.SubscribeMenu(gameObject);
            else
                Debug.LogError($"Menu Root {name} has no assigned menu. May have been deleted during menu updates!", this);
        }
    }
}