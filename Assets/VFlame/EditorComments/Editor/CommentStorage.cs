// Created by Luke Jones 12/04/2026
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VFlame.EditorComments.Editor
{
    /// <summary>
    /// Stores comments made
    /// </summary>
    public class CommentStorage
    {
        /// <summary>
        /// List of all comments on objects
        /// </summary>
        CommentDictionary comments = null;

        /// <summary>
        /// Saves any changes to a comment object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public CommentObject GetComments(UnityEngine.Object obj)
        {
            if (obj == null)
                return null;

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            string idString = id.ToString();

            // Search the comments for a comment for this object. Unfortunately, GlobalObjectId doesn't serialize so SerializableDictionaries don't work :(
            if (comments != null && comments.TryGetValue(idString, out var comment))
            {
                comment.isAsset = true;
                return comment;
            }

            // Compute the name so we don't need to re-compute it constantly.
            string name = "Comments_" + idString;

            // Try load the asset from the project. If successful, store it in the dictionary
            comment = AssetDatabase.LoadAssetAtPath<CommentObject>(CommentsEditorWindow.SAVE_PATH + "/" + name + ".asset");
            if (comment)
            {
                // Store in dictionary before returning for faster accessing
                comments ??= new CommentDictionary();
                comments[comment.objectId] = comment;
                comment.isAsset = true;
                return comment;
            }

            comment = ScriptableObject.CreateInstance<CommentObject>();
            comment.name = name;
            comment.objectId = id.ToString();
            comment.isAsset = false;

            return comment;
        }

        /// <summary>
        /// Saves any changes made to a comment object
        /// </summary>
        /// <param name="comment"></param>
        public void SaveComment(CommentObject comment)
        {
            // Don't save an empty comment :P
            if (comment == null ||
                comment.comment.IsNullOrEmpty())
            {
                // If its an asset'ed comment, delete it.
                if (comment.isAsset)
                {
                    comments.Remove(comment.objectId);
                    AssetDatabase.DeleteAsset(CommentsEditorWindow.SAVE_PATH + "/" + comment.name + ".asset");
                }

                return;
            }

            // If this is not yet an asset, convert it to one.
            if (comment.isAsset == false)
            {
                AssetDatabase.CreateAsset(comment, CommentsEditorWindow.SAVE_PATH + "/" + comment.name + ".asset");
                comment.isAsset = true;
            }

            // Store the comment in the object
            comments ??= new CommentDictionary();
            comments[comment.objectId] = comment;

            EditorUtility.SetDirty(comment);
        }
    }

    public class CommentDictionary : Dictionary<string, CommentObject> { }
}