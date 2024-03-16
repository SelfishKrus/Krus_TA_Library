using UnityEditor;
using UnityEngine;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerFloat4
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            Vector4 newValyue;
            bool changed;

            switch (content.Description.WidgetType)
            {
                case SubstanceWidgetType.Color:
                    changed = DrawColorPicker(valueProperty, content, out newValyue);
                    break;

                default:
                    changed = DrawDefault(valueProperty, content, out newValyue);
                    break;
            }

            if (changed)
                handler.SetInputFloat4(inputID, newValyue);

            return changed;
        }

        /// <summary>
        /// Renders custome GUI for input float4 as color.
        /// </summary>
        /// <param name="position">GUI position rect.</param>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <param name="description">Description for the target input.</param>
        private static bool DrawColorPicker(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector4 newValue)
        {
            var previewValue = valueProperty.vector4Value;

            var color = new Vector4(previewValue.x, previewValue.y, previewValue.z, previewValue.w);
            newValue = EditorGUILayout.ColorField(content, color);

            if (color != newValue)
            {
                valueProperty.vector4Value = new Vector4(newValue.x, newValue.y, newValue.z, newValue.w);
                return true;
            }

            return false;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector4 newValue)
        {
            var oldValue = valueProperty.vector4Value;
            newValue = EditorGUILayout.Vector4Field(content, oldValue);

            if (oldValue != newValue)
            {
                valueProperty.vector4Value = newValue;
                return true;
            }

            return false;
        }
    }
}