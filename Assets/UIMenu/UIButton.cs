using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VFlame.UI
{
    /// <summary>
    /// UIButton that interfaces with UIManager
    /// </summary>
    public class UIButton : Button
    {
        public override bool IsInteractable()
        {
            // Override IsInteractable to disable interactability when transitioning UI elements.
            return !UIManager.IsTransitioning && base.IsInteractable();
        }
    }
}