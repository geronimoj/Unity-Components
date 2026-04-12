using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorComments.Editor
{
    /// <summary>
    /// Stores comments made
    /// </summary>
    public class CommentStorage : Factories.SingletonFactory<CommentStorage>
    {
        [SerializeField] List<CommentObject> comments = null;

        public CommentObject GetComments(UnityEngine.Object obj)
        {
            if (obj == null)
                return null;

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(obj);

            // Search the comments for a comment for this object. Unfortunately, GlobalObjectId doesn't serialize so SerializableDictionaries don't work :(
            if (comments != null)
                foreach (var comment in comments)
                {
                    if (comment.objectId == id.ToString())
                    {
                        comment.isAsset = true;
                        return comment;
                    }
                }

            var instance = CreateInstance<CommentObject>();
            instance.name = "Comments_" + obj.name;
            instance.objectId = id.ToString();
            instance.isAsset = false;

            return instance;
        }

        public void SaveComment(CommentObject comment)
        {
            // Don't save an empty comment :P
            if (comment == null ||
                comment.comment.IsNullOrEmpty())
            {
                // If its an asset'ed comment, delete it.
                if (comment.isAsset)
                {
                    comments.Remove(comment);
                    AssetDatabase.RemoveObjectFromAsset(comment);

                    EditorUtility.SetDirty(this);
                    EditorUtility.SetDirty(comment);
                }

                return;
            }

            // If this is not yet an asset, convert it to one.
            if (comment.isAsset == false)
            {
                //AssetDatabase.CreateAsset(comment, CommentsEditorWindow.SAVE_PATH + "/" + comment.name + ".asset");
                AssetDatabase.AddObjectToAsset(comment, CommentsEditorWindow.SAVE_PATH + "/CommentStorage.asset");
                comment.isAsset = true;
            }

            // Store the comment in the object
            comments ??= new List<CommentObject>();
            if (!comments.Contains(comment))
                comments.Add(comment);

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(comment);
        }
    }
}