using UnityEditor;

[CustomEditor(typeof(OnGround_ElseAirborne))]
[CanEditMultipleObjects]
public class OnGroundElseAirborneEditor : Editor
{
    SerializedProperty ifState;
    SerializedProperty elseState;
    private void OnEnable()
    {
        ifState = serializedObject.FindProperty("_ifState");
        elseState = serializedObject.FindProperty("_elseState");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(ifState);
        EditorGUILayout.PropertyField(elseState);
        serializedObject.ApplyModifiedProperties();
    }
}
