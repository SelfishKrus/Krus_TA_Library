using UnityEditor;
using UnityEngine;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerFloat3
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            Vector3 newValue;
            bool changed;

            switch (content.Description.WidgetType)
            {
                case SubstanceWidgetType.Color:
                    changed = DrawColorPicker(valueProperty, content, out newValue);
                    break;

                default:
                    changed = DrawDefault(valueProperty, content, out newValue);
                    break;
            }

            if (changed)
                handler.SetInputFloat3(inputID, newValue);

            return changed;
        }

        /// <summary>
        /// Renders custome GUI for input float3 as color.
        /// </summary>
        /// <param name="position">GUI position rect.</param>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <param name="description">Description for the target input.</param>
        private static bool DrawColorPicker(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector3 newValue)
        {
            var previewValue = valueProperty.vector3Value;

            var color = new Color(previewValue.x, previewValue.y, previewValue.z, 1);
            var newColor = EditorGUILayout.ColorField(content, color, false, false, false);
            newValue = new Vector3();

            if (color != newColor)
            {
                newValue = new Vector4(newColor.r, newColor.g, newColor.b, newColor.a);
                valueProperty.vector3Value = newValue;
                return true;
            }

            return false;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector3 newValue)
        {
            var previewValue = valueProperty.vector3Value;
            newValue = EditorGUILayout.Vector3Field(content, previewValue);

            if (newValue != previewValue)
            {
                valueProperty.vector3Value = newValue;
                return true;
            }

            return false;
        }
    }
}