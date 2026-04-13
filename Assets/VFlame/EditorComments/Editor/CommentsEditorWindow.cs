// Created by Luke Jones 12/04/2026
using UnityEngine;
using UnityEditor;

namespace VFlame.EditorComments.Editor
{
    /// <summary>
    /// Editor window for displaying comments on selected objects
    /// </summary>
    public class CommentsEditorWindow : EditorWindow
    {
        /// <summary>
        /// Max selectable objects. Limited to 1 beacuse of how display is defined. People probably don't care if they have multiple objects selected anyways :P
        /// </summary>
        public const int MAX_SELECTABLE_OBJECTS = 1;
        /// <summary>
        /// Save path for resources
        /// </summary>
        public const string SAVE_PATH = "Assets/VFlame/EditorComments/Editor/Resources/Editor/Comments";

        /// <summary>
        /// The object holding the comments
        /// </summary>
        CommentStorage storage;

        /// <summary>
        /// Array of the currently selected objects in the editor
        /// </summary>
        Object[] selected;
        /// <summary>
        /// Array containing the comments of the selected objects
        /// </summary>
        CommentObject[] comments;

        [MenuItem("Window/General/Comments")]
        static void OpenCommentWindow()
        {
            // Create the window & load the storage object
            CommentsEditorWindow window = GetWindow<CommentsEditorWindow>("Comments");
            window.LoadStorage();
            window.OnSelectionChange();
            window.Show();
        }

        private void LoadStorage()
        {
            storage ??= new CommentStorage();
        }

        private void OnSelectionChange()
        {
            // Make sure the storage object is loaded. Mostly for the case where the window is auto-opened but also to support if the asset accidentally gets
            // deleted or otherwise the reference is lost
            LoadStorage();

            selected = Selection.objects;

            // If there are too many objects selected, don't bother with comments
            if (selected.Length > MAX_SELECTABLE_OBJECTS)
            {
                comments = new CommentObject[0];
            }
            else
            {
                // Load the comments into memory
                comments = new CommentObject[selected.Length];

                for (int i = 0; i < comments.Length; i++)
                    comments[i] = storage.GetComments(selected[i]);
            }

            // Request a repaint on the UI element so that the window updates in real time.
            Repaint();
        }

        private void OnGUI()
        {
            // If nothing is selected, notify the user nothing is selected
            if (selected == null || comments == null || selected.Length == 0)
            {
                // Yes, to position the lable correctly, I am using another label field, because I couldn't figure out how to get it to position correctly
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space((position.width / 2) - 54);
                EditorGUILayout.LabelField("Select an Object", GUILayout.Width(position.width), GUILayout.Height(position.height));
                EditorGUILayout.EndHorizontal();

                return;
            }

            // If there are too many objects selected, don't render the comment window.
            // This is to save on performance if someone accidentally selects a lot of objects
            if (selected.Length > MAX_SELECTABLE_OBJECTS)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space((position.width / 2) - 80);
                EditorGUILayout.LabelField("Multiple Objects Selected", GUILayout.Width(position.width), GUILayout.Height(position.height));
                EditorGUILayout.EndHorizontal();

                return;
            }

            // Render the comment for each selected object.
            foreach (var commentObj in comments)
            {
                // Parse the objectId back to a GlobalObjectId so we can sanity check it. (Cheaper than recomputing it)
                GlobalObjectId.TryParse(commentObj.objectId, out var id);

                // If the Asset GUID is null & it's a scene asset, we cannot track it until the scene is saved!
                if (id.identifierType == 2 && id.assetGUID == default)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space((position.width / 2) - 93);
                    EditorGUILayout.LabelField("Save Scene to Create Comments", GUILayout.Width(position.width), GUILayout.Height(position.height));
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                // Draw the comment in a text area, the size of the window
                string newComment = EditorGUILayout.TextArea(commentObj.comment, GUILayout.MinHeight(position.height - 5));

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