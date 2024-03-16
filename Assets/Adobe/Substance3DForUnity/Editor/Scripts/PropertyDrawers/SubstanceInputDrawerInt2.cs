using UnityEditor;
using UnityEngine;
using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawerInt2
    {
        private static readonly string[] _resulutions = { "256", "512", "1024", "2048", "4096" };

        public static bool DrawInput(SerializedProperty valueProperty, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            Vector2Int newValue;
            bool changed;

            if (IsSizeAttribute(content))
            {
                changed = DrawResolutionSelection(valueProperty, content, out newValue);
            }
            else
            {
                changed = DrawDefault(valueProperty, content, out newValue);
            }

            if (changed)
                handler.SetInputInt2(inputID, newValue);

            return changed;
        }

        private static bool DrawResolutionSelection(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector2Int newValue)
        {
            Vector2Int oldValue = valueProperty.vector2IntValue;
            var currentIndex = GetEnumIndex(oldValue);
            int newIndex = EditorGUILayout.Popup("Output Resolution", currentIndex, _resulutions);

            if (currentIndex != newIndex)
            {
                newValue = GetValueFromIndex(newIndex);
                valueProperty.vector2IntValue = newValue;
                return true;
            }

            newValue = new Vector2Int();
            return false;
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content, out Vector2Int newValue)
        {
            Vector2Int oldValue = valueProperty.vector2IntValue;
            newValue = EditorGUILayout.Vector2IntField(content, oldValue);

            if (newValue != oldValue)
            {
                valueProperty.vector2IntValue = newValue;
                return true;
            }

            return false;
        }

        private static bool IsSizeAttribute(GUIContent content)
        {
            return content.text == "$outputsize";
        }

        private static int GetEnumIndex(Vector2Int data)
        {
            switch (data.x)
            {
                case 8: return 0;
                case 9: return 1;
                case 10: return 2;
                case 11: return 3;
                case 12: return 4;
                default:
                    return 0;
            }
        }

        private static Vector2Int GetValueFromIndex(int index)
        {
            switch (index)
            {
                case 0: return new Vector2Int(8, 8);
                case 1: return new Vector2Int(9, 9);
                case 2: return new Vector2Int(10, 10);
                case 3: return new Vector2Int(11, 11);
                case 4: return new Vector2Int(12, 12);
                default:
                    return new Vector2Int(8, 8);
            }
        }
    }
}