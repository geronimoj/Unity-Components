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
    public class CommentStorage : ScriptableObject
    {
        /// <summary>
        /// List of all comments on objects
        /// </summary>
        [SerializeField] CommentDictionary comments = null;

        public CommentObject GetComments(UnityEngine.Object obj)
        {
            if (obj == null)
                return null;

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(obj);

            // Search the comments for a comment for this object. Unfortunately, GlobalObjectId doesn't serialize so SerializableDictionaries don't work :(
            if (comments != null && comments.TryGetValue(id.ToString(), out var comment))
            {
                comment.isAsset = true;
                return comment;
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
                    comments.Remove(comment.objectId);
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
            comments ??= new CommentDictionary();
            comments[comment.objectId] = comment;

            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(comment);
        }
    }

    /// <summary>
    /// Serializable dictionary for comment storage
    /// </summary>
    [Serializable]
    public class CommentDictionary : Dictionary<string, CommentObject>, ISerializationCallbackReceiver
    {
        [SerializeField] string[] objectIds;
        [SerializeField] CommentObject[] comments;

        public CommentDictionary()
        {
        }

        public CommentDictionary(IDictionary<string, CommentObject> dict) : base(dict.Count)
        {
            foreach (var kvp in dict)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        public Dictionary<string, CommentObject> GetSerializedData()
        {
            Dictionary<string, CommentObject> dict = null;
            if (objectIds != null && comments != null && objectIds.Length == comments.Length)
            {
                dict = new Dictionary<string, CommentObject>();
                int n = objectIds.Length;
                for (int i = 0; i < n; ++i)
                {
                    dict.Add(objectIds[i], comments[i]);
                }
            }
            else
            {
                OnBeforeSerialize();
                dict = new Dictionary<string, CommentObject>(this);
            }
            return dict;
        }

        public void SetSerializedData(Dictionary<string, CommentObject> dict)
        {
            int n = dict.Count;
            objectIds = new string[n];
            comments = new CommentObject[n];
            this.Clear();

            int i = 0;
            foreach (var kvp in dict)
            {
                objectIds[i] = kvp.Key;
                comments[i] = kvp.Value;
                this[objectIds[i]] = comments[i];
                ++i;
            }
        }

        public void CopyFrom(IDictionary<string, CommentObject> dict)
        {
            //Debug.Log("Copy From:" + this);
            this.Clear();
            foreach (var kvp in dict)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        public void OnAfterDeserialize()
        {
            //Debug.Log("After Deser:" + this);
            if (objectIds != null && comments != null && objectIds.Length == comments.Length)
            {
                this.Clear();
                int n = objectIds.Length;
                for (int i = 0; i < n; ++i)
                {
                    // Skip null keys to prevent ArgumentNullException
                    if (objectIds[i] != null)
                    {
                        this[objectIds[i]] = comments[i];
                    }
                    else
                    {
                        Debug.LogError($"SerializableDictionary ({this.GetType().Name}) found null key at index {i} during deserialization. Value: {comments[i]}. Skipping entry.");
                    }
                }

                objectIds = null;
                comments = null;
            }

        }

        public void OnBeforeSerialize()
        {
            //Debug.Log("Before Ser:" + this);
            int n = this.Count;
            objectIds = new string[n];
            comments = new CommentObject[n];

            int i = 0;
            foreach (var kvp in this)
            {
                objectIds[i] = kvp.Key;
                comments[i] = kvp.Value;
#if UNITY_EDITOR
                if (objectIds[i] == null || objectIds[i] is null)
                {
                    Debug.LogError($"SerializableDictionary ({this.GetType().Name}) found null key at index {i} during serialization.");
                }
#endif
                ++i;
            }
        }
    }
}