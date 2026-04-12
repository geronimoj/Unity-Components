using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorComments.Editor
{
    /// <summary>
    /// Stores the comments for an object
    /// </summary>
    public class CommentObject : ScriptableObject
    {
        /// <summary>
        /// The Id of the object the comment is ascociated with
        /// </summary>
        public string objectId;

        /// <summary>
        /// Is this object an asset
        /// </summary>
        [System.NonSerialized]
        public bool isAsset = false;

        /// <summary>
        /// The comment on the object
        /// </summary>
        [TextArea]
        public string comment;
    }
}