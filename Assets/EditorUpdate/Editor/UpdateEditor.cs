using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExampleMono))]
public class UpdateEditor : Editor
{
    DateTime _curTime = DateTime.MinValue;
    float tick = 0f;
    /// <summary>
    /// Current time for this window.
    /// </summary>
    public float Tick => tick;

    private void OnSceneGUI()
    {   //Calculate time that has passed
        if (_curTime == DateTime.MinValue)
            _curTime = DateTime.Now;

        float delta = (float)(DateTime.Now - _curTime).TotalSeconds;
        tick += delta;

        _curTime = DateTime.Now;
        //Update
        EditorUpdate(delta);

        SceneView.RepaintAll(); // Force UI update
    }

    protected virtual void EditorUpdate(float delta)
    {
        // An actually consistent update call!
        Debug.Log("Update!");
    }
}
