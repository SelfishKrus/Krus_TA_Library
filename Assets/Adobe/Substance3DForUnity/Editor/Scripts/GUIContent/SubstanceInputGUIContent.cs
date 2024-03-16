using Adobe.Substance.Input.Description;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Custome GUIContent class that provides extra information for drawing input parameters.
    /// </summary>
    internal class SubstanceInputGUIContent : GUIContent
    {
        /// <summary>
        /// Description info for the input SerializedProperty.
        /// </summary>
        public SubstanceInputDescription Description;

        public SerializedProperty DataProp;

        public SubstanceInputGUIContent(SubstanceInputDescription description, SerializedProperty dataProp) : base(description.Label, description.Identifier)
        {
            Description = description;
            DataProp = dataProp;
        }

        public SubstanceInputGUIContent(SubstanceInputDescription description, SerializedProperty dataProp, string text) : base(text)
        {
            Description = description;
            DataProp = dataProp;
        }
    }
}