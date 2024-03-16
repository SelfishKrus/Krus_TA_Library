using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Static class with utility methods commonly used to draw substance properties and UI.
    /// </summary>
    internal static class EditorDrawUtilities
    {
        private static readonly string[] _resolutions = { "256", "512", "1024", "2048", "4096" };

        public static void DrawResolutionSelection(SerializedProperty property, GUIContent content, params GUILayoutOption[] options)
        {
            Vector2Int oldValue = property.vector2IntValue;
            var currentIndex = GetEnumIndex(oldValue);
            int newIndex = EditorGUILayout.Popup(content, currentIndex, _resolutions, options);

            if (currentIndex != newIndex)
            {
                Vector2Int newValue = GetValueFromIndex(newIndex);
                property.vector2IntValue = newValue;
            }
        }

        private static int GetEnumIndex(Vector2Int data)
        {
            switch (data.x)
            {
                case 8:
                    return 0;

                case 9:
                    return 1;

                case 10:
                    return 2;

                case 11:
                    return 3;

                case 12:
                    return 4;

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