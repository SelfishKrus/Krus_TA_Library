using UnityEditor;
using UnityEngine;
using Adobe.Substance.Input.Description;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Drawer helper for int substance inputs.
    /// </summary>
    internal static class SubstanceInputDrawerInt
    {
        /// <summary>
        /// Draws int input.
        /// </summary>
        /// <param name="valueProperty"></param>
        /// <param name="content"></param>
        /// <returns>True if value changed.</returns>
        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            int value;
            bool changed;

            if (content.Description.Label == "$randomseed")
            {
                changed = DrawRandomSeedButton(valueProperty, content, out value);
            }
            else
            {
                switch (content.Description.WidgetType)
                {
                    case SubstanceWidgetType.ToggleButton:
                        changed = DrawToggleButton(valueProperty, content as SubstanceIntGUIContent, out value);
                        break;

                    case SubstanceWidgetType.Slider:
                        changed = DrawSlider(valueProperty, content as SubstanceIntGUIContent, out value);
                        break;

                    case SubstanceWidgetType.ComboBox:
                        changed = DrawComboBox(valueProperty, content as SubstanceIntGUIContent, out value);
                        break;

                    case SubstanceWidgetType.EnumButton:
                        changed = DrawEnumButton(valueProperty, content as SubstanceIntGUIContent, out value);
                        break;

                    default:
                        changed = DrawDefault(valueProperty, content, out value);
                        break;
                }
            }

            if (changed)
                handler.SetInputInt(inputID, value);

            return changed;
        }

        /// <summary>
        /// Renders the int input as a toggle button.
        /// </summary>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <returns>True if value changed.</returns>
        private static bool DrawToggleButton(SerializedProperty valueProperty, SubstanceIntGUIContent content, out int newValue)
        {
            newValue = 0;
            var oldValue = valueProperty.intValue != 0;
            var newValueToggle = EditorGUILayout.Toggle(content, oldValue);

            if (oldValue != newValueToggle)
            {
                newValue = newValueToggle ? 1 : 0;
                valueProperty.intValue = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renders the int input as a slider.
        /// </summary>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <returns>True if value changed.</returns>
        private static bool DrawSlider(SerializedProperty valueProperty, SubstanceIntGUIContent content, out int newValue)
        {
            var numDescription = content.NumericalDescription;

            var maxValue = numDescription.MaxValue;
            var minValue = numDescription.MinValue;

            var oldValue = valueProperty.intValue;
            newValue = EditorGUILayout.IntSlider(content, oldValue, minValue, maxValue);

            if (oldValue != newValue)
            {
                valueProperty.intValue = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renders the int input as combo box.
        /// </summary>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <returns>True if value changed.</returns>
        private static bool DrawComboBox(SerializedProperty valueProperty, SubstanceIntGUIContent content, out int newValue)
        {
            var specializedContent = content as SubstanceIntComboBoxGUIContent;

            var oldValue = valueProperty.intValue;
            newValue = EditorGUILayout.IntPopup(content, oldValue, specializedContent.EnumValuesGUI, specializedContent.EnumValues);

            if (oldValue != newValue)
            {
                valueProperty.intValue = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renders the int input an enum button.
        /// </summary>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <returns>True if value changed.</returns>
        private static bool DrawEnumButton(SerializedProperty valueProperty, SubstanceIntGUIContent content, out int newValue)
        {
            var specializedContent = content as SubstanceIntEnumButtonGUIContent;

            var oldValue = valueProperty.intValue;
            newValue = EditorGUILayout.IntPopup(content, oldValue, specializedContent.EnumValuesGUI, specializedContent.EnumValues);

            if (oldValue != newValue)
            {
                valueProperty.intValue = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Default input render.
        /// </summary>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <returns>True if value changed.</returns>
        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out int newValue)
        {
            var oldValue = valueProperty.intValue;
            newValue = EditorGUILayout.IntField(content, oldValue);

            if (oldValue != newValue)
            {
                valueProperty.intValue = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws the random seed button.
        /// </summary>
        /// <param name="valueProperty">Value property.</param>
        /// <param name="content">GUI content.</param>
        /// <returns>True if value changed.</returns>
        private static bool DrawRandomSeedButton(SerializedProperty valueProperty, SubstanceInputGUIContent content, out int newValue)
        {
            bool result = false;

            newValue = valueProperty.intValue;
            int minimum = 0;
            int maximum = 10000;

            int labelWidth = (int)EditorGUIUtility.labelWidth - 15;
            int fieldWidth = 50;

            content.text = "Random Seed";
            content.tooltip = "$randomseed: the overall random aspect of the texture";

            GUILayout.BeginHorizontal();
            {
                int buttonWidth = (int)EditorGUIUtility.currentViewWidth - labelWidth - fieldWidth - 60;

                EditorGUILayout.LabelField(content, GUILayout.Width(labelWidth), GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Randomize", GUILayout.Width(buttonWidth)))
                {
                    newValue = UnityEngine.Random.Range(minimum, maximum);
                    valueProperty.intValue = newValue;
                    result = true;
                }

                EditorGUI.BeginChangeCheck();

                newValue = EditorGUILayout.IntField(newValue, GUILayout.Width(fieldWidth));

                if (EditorGUI.EndChangeCheck())
                {
                    newValue = (newValue < minimum) ? minimum : (newValue > maximum) ? maximum : newValue;
                    valueProperty.intValue = newValue;
                    result = true;
                }
            }

            GUILayout.EndHorizontal();
            return result;
        }
    }
}