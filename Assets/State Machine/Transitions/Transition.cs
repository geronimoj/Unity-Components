using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base transition. All transitions inherit from this class
/// </summary>
[CreateAssetMenu(fileName = "BaseTransition", menuName = "Transitions/Transition", order = 0)]
public class Transition : ScriptableObject
{
    /// <summary>
    /// The state we should transition to if this transition returns true
    /// </summary>
    public State targetState;
   /// <summary>
   /// An overridable function for creating transitions
   /// </summary>
   /// <param name="ctrl">A reference to the player controller</param>
   /// <returns>Returns false by default</returns>
    public virtual bool ShouldTransition(ref PlayerController ctrl) { return false; }
}
