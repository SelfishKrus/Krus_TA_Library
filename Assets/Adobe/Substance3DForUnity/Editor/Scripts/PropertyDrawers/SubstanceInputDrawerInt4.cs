using UnityEditor;
using Adobe.Substance;
using UnityEngine;
using Adobe.Substance.Input.Description;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerInt4
    {
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            int value0;
            int value1;
            int value2;
            int value3;
            bool changed;

            switch (content.Description.WidgetType)
            {
                case SubstanceWidgetType.Slider:
                    changed = DrawSliderWidget(valueProperty, content as SubstanceInt4GUIContent, out value0, out value1, out value2, out value3);
                    break;

                case SubstanceWidgetType.Color:
                    changed = DrawColorWidget(valueProperty, content as SubstanceInt4GUIContent, out value0, out value1, out value2, out value3);
                    break;
                //TODO: Add edge cases here.
                default:
                    changed = DrawDefault(valueProperty, content as SubstanceInt4GUIContent, out value0, out value1, out value2, out value3);
                    break;
            }

            if (changed)
                handler.SetInputInt4(inputID, value0, value1, value2, value3);

            return changed;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInt4GUIContent content, out int newValue0, out int newValue1, out int newValue2, out int newValue3)
        {
            bool result = false;

            var value0 = valueProperty?.FindPropertyRelative("Data0");
            var value1 = valueProperty?.FindPropertyRelative("Data1");
            var value2 = valueProperty?.FindPropertyRelative("Data2");
            var value3 = valueProperty?.FindPropertyRelative("Data3");

            var previewValue0 = value0.intValue;
            var previewValue1 = value1.intValue;
            var previewValue2 = value2.intValue;
            var previewValue3 = value3.intValue;

            newValue0 = EditorGUILayout.IntField(content, previewValue0);
            newValue1 = EditorGUILayout.IntField(content, previewValue1);
            newValue2 = EditorGUILayout.IntField(content, previewValue2);
            newValue3 = EditorGUILayout.IntField(content, previewValue3);

            if (newValue0 != previewValue0)
            {
                value0.intValue = newValue0;
                result = true;
            }

            if (newValue1 != previewValue1)
            {
                value1.intValue = newValue1;
                result = true;
            }

            if (newValue2 != previewValue2)
            {
                value2.intValue = newValue2;
                result = true;
            }

            if (newValue3 != previewValue3)
            {
                value3.intValue = newValue3;
                result = true;
            }

            return result;
        }

        private static bool DrawSliderWidget(SerializedProperty valueProperty, SubstanceInt4GUIContent content, out int newValue0, out int newValue1, out int newValue2, out int newValue3)
        {
            bool result = false;

            int rightValue0 = 100;
            int rightValue1 = 100;
            int rightValue2 = 100;
            int rightValue3 = 100;

            int leftValue0 = 0;
            int leftValue1 = 0;
            int leftValue2 = 0;
            int leftValue3 = 0;

            var int4NumbericalDescription = content.NumericalDescription;

            if (int4NumbericalDescription != null)
            {
                leftValue0 = int4NumbericalDescription.MinValue0;
                leftValue1 = int4NumbericalDescription.MinValue1;
                leftValue2 = int4NumbericalDescription.MinValue2;
                leftValue3 = int4NumbericalDescription.MinValue3;

                rightValue0 = int4NumbericalDescription.MaxValue0;
                rightValue1 = int4NumbericalDescription.MaxValue1;
                rightValue2 = int4NumbericalDescription.MaxValue2;
                rightValue3 = int4NumbericalDescription.MaxValue3;
            }

            var value0 = valueProperty?.FindPropertyRelative("Data0");
            var value1 = valueProperty?.FindPropertyRelative("Data1");
            var value2 = valueProperty?.FindPropertyRelative("Data2");
            var value3 = valueProperty?.FindPropertyRelative("Data3");

            var previewValue0 = value0.intValue;
            var previewValue1 = value1.intValue;
            var previewValue2 = value2.intValue;
            var previewValue3 = value3.intValue;

            newValue0 = EditorGUILayout.IntSlider(content, previewValue0, leftValue0, rightValue0);
            newValue1 = EditorGUILayout.IntSlider(content, previewValue1, leftValue1, rightValue1);
            newValue2 = EditorGUILayout.IntSlider(content, previewValue2, leftValue2, rightValue2);
            newValue3 = EditorGUILayout.IntSlider(content, previewValue3, leftValue3, rightValue3);

            if (newValue0 != previewValue0)
            {
                value0.intValue = newValue0;
                result = true;
            }

            if (newValue1 != previewValue1)
            {
                value1.intValue = newValue1;
                result = true;
            }

            if (newValue2 != previewValue2)
            {
                value2.intValue = newValue2;
                result = true;
            }

            if (newValue3 != previewValue3)
            {
                value3.intValue = newValue3;
                result = true;
            }

            return result;
        }

        private static bool DrawColorWidget(SerializedProperty valueProperty, SubstanceInt4GUIContent content, out int newValue0, out int newValue1, out int newValue2, out int newValue3)
        {
            bool result = false;

            var value0 = valueProperty?.FindPropertyRelative("Data0");
            var value1 = valueProperty?.FindPropertyRelative("Data1");
            var value2 = valueProperty?.FindPropertyRelative("Data2");
            var value3 = valueProperty?.FindPropertyRelative("Data3");

            var previewValue0 = value0.intValue;
            var previewValue1 = value1.intValue;
            var previewValue2 = value2.intValue;
            var previewValue3 = value3.intValue;

            var floatValue0 = ((float)previewValue0) / 255f;
            var floatValue1 = ((float)previewValue1) / 255f;
            var floatValue2 = ((float)previewValue2) / 255f;
            var floatValue3 = ((float)previewValue3) / 255f;

            Color color = new Color(floatValue0, floatValue1, floatValue2, floatValue3);

            var newColor = EditorGUILayout.ColorField(content, color);

            newValue0 = (int)newColor.r * 255;
            newValue1 = (int)newColor.g * 255;
            newValue2 = (int)newColor.b * 255;
            newValue3 = (int)newColor.a * 255;

            if (newValue0 != previewValue0)
            {
                value0.intValue = newValue0;
                result = true;
            }

            if (newValue1 != previewValue1)
            {
                value1.intValue = newValue1;
                result = true;
            }

            if (newValue2 != previewValue2)
            {
                value2.intValue = newValue2;
                result = true;
            }

            if (newValue3 != previewValue3)
            {
                value3.intValue = newValue3;
                result = true;
            }

            return result;
        }
    }
}