using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UpdateEditorWindow : EditorWindow
{
    DateTime _curTime = DateTime.MinValue;
    float tick = 0f;
    /// <summary>
    /// Current time for this window.
    /// </summary>
    public float Tick => tick;

    #region Unimportant Stuff
    [MenuItem("Window/Editor Window With Update")]
    static void Init()
    {
        UpdateEditorWindow window = GetWindow<UpdateEditorWindow>();
        window.Show();
    }
    // Window has been selected
    void OnFocus()
    {
        SceneView.duringSceneGui -= InternalUpdate;
        SceneView.duringSceneGui += InternalUpdate;
    }
    void OnDestroy()
    {
        SceneView.duringSceneGui -= InternalUpdate;
    }
    #endregion

    void InternalUpdate(SceneView sceneView)
    {   //Calculate time that has passed
        if (_curTime == DateTime.MinValue)
            _curTime = DateTime.Now;

        float delta = (float)(DateTime.Now - _curTime).TotalSeconds;
        tick += delta;

        _curTime = DateTime.Now;

        EditorUpdate(delta);

        SceneView.RepaintAll(); // Force another upate
    }

    protected virtual void EditorUpdate(float delta)
    {
        Debug.Log("Update!");
    }
}
