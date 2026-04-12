using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorComments.Editor
{
    /// <summary>
    /// Editor window for displaying comments on selected objects
    /// </summary>
    public class CommentsEditorWindow : EditorWindow
    {
        public const int MAX_SELECTABLE_OBJECTS = 1;
        public const string STORAGE_PATH = "Editor/CommentStorage";
        public const string SAVE_PATH = "Assets/EditorComments/Editor/Resources/Editor";

        public CommentStorage storage;
        Object[] selected;

        [MenuItem("Window/General/Comments")]
        static void OpenCommentWindow()
        {
            CommentsEditorWindow window = GetWindow<CommentsEditorWindow>();
            window.storage = Resources.Load<CommentStorage>(STORAGE_PATH);

            if (window.storage == null)
            {
                AssetDatabase.CreateAsset(new CommentStorage(), SAVE_PATH + "/CommentStorage.asset");
                window.storage = Resources.Load<CommentStorage>(STORAGE_PATH);

                EditorUtility.SetDirty(window.storage);
                AssetDatabase.SaveAssets();
            }

            window.Show();
        }

        private void OnSelectionChange()
        {
            selected = Selection.objects;

            Repaint();
        }

        private void OnGUI()
        {
            // Don't render anything
            if (selected == null)
                return;

            // If there are too many objects selected, don't render the comment window.
            // This is to save on performance if someone accidentally selects a lot of objects
            if (selected.Length > MAX_SELECTABLE_OBJECTS)
            {
                EditorGUILayout.LabelField("Multiple Objects Selected");
                return;
            }

            // Render the comment for each selected object.
            foreach (var obj in selected)
            {
                var commentObj = storage.GetComments(obj);

                EditorGUILayout.LabelField("Selected: " + obj.name);
                string newComment = EditorGUILayout.TextArea(commentObj.comment, GUILayout.MinHeight(80f));

                // If the comment has changed, saved it
                if (newComment != commentObj.comment)
                {
                    commentObj.comment = newComment;
                    storage.SaveComment(commentObj);
                }
            }
        }
    }
}