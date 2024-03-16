using UnityEditor;
using UnityEngine;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerFloat2
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            Vector2 newValue;
            bool changed;

            switch (content.Description.WidgetType)
            {
                default:
                    changed = DrawDefault(valueProperty, content, out newValue);
                    break;
            }

            if (changed)
                handler.SetInputFloat2(inputID, newValue);

            return changed;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector2 newValue)
        {
            bool result = false;

            var previewValue = valueProperty.vector2Value;
            newValue = EditorGUILayout.Vector2Field(content, previewValue);

            if (newValue != previewValue)
            {
                valueProperty.vector2Value = newValue;
                result = true;
            }

            return result;
        }
    }
}