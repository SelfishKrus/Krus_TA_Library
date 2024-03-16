using UnityEditor;
using UnityEngine;
using Adobe.Substance.Input.Description;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerFloat
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            float newValue;
            bool changed = false;

            switch (content.Description.WidgetType)
            {
                case SubstanceWidgetType.Slider:
                    changed = DrawSlider(valueProperty, content as SubstanceFloatGUIContent, out newValue);
                    break;

                default:
                    changed = DrawDefault(valueProperty, content, out newValue);
                    break;
            }

            if (changed)
                handler.SetInputFloat(inputID, newValue);

            return changed;
        }

        private static bool DrawSlider(SerializedProperty valueProperty, SubstanceFloatGUIContent content, out float newValue)
        {
            var floatInputDesc = content.NumericalDescription;

            var maxValue = floatInputDesc.MaxValue;
            var minValue = floatInputDesc.MinValue;
            var sliderClamp = maxValue != minValue;

            var oldValue = valueProperty.floatValue;

            newValue = EditorGUILayout.Slider(content, oldValue, sliderClamp ? minValue : 0, sliderClamp ? maxValue : 50);

            if (oldValue != newValue)
            {
                valueProperty.floatValue = newValue;
                return true;
            }

            return false;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out float newValue)
        {
            var oldValue = valueProperty.floatValue;
            newValue = EditorGUILayout.FloatField(content, oldValue);

            if (oldValue != newValue)
            {
                valueProperty.floatValue = newValue;
                return true;
            }

            return false;
        }
    }
}