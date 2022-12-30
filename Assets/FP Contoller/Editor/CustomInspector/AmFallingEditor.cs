using UnityEditor;
/// <summary>
/// A custom editor class for the AmFalling transition.
/// This is because, for consistency, AmFalling inherits from OnGround_ElseAirborne so it can use its functions.
/// This causes the ifState & elseState to appear, confusing the developer. As such, the custom inspector servers,
/// to hide those variables from the developer
/// </summary>
[CustomEditor(typeof(AmFalling))]
[CanEditMultipleObjects]
public class AmFallingEditor : Editor
{/// <summary>
/// The target state
/// </summary>
    SerializedProperty targetState;
    /// <summary>
    /// Assign targetState to the correct variable
    /// </summary>
    private void OnEnable()
    {
        targetState = serializedObject.FindProperty("targetState");
    }
    /// <summary>
    /// Update the Inpector and do the variable things
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(targetState);
        serializedObject.ApplyModifiedProperties();
    }
}
