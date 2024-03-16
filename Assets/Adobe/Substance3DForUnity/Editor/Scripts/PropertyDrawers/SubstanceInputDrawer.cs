using Adobe.Substance;
using UnityEditor;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceInputDrawer
    {
        public static bool DrawInput(SerializedProperty property, SubstanceInputGUIContent content, SubstanceNativeGraph handler, int inputID)
        {
            switch (content.Description.Type)
            {
                case SubstanceValueType.Float:
                    return SubstanceInputDrawerFloat.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.Float2:
                    return SubstanceInputDrawerFloat2.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.Float3:
                    return SubstanceInputDrawerFloat3.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.Float4:
                    return SubstanceInputDrawerFloat4.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.Int:
                    return SubstanceInputDrawerInt.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.Int2:
                    return SubstanceInputDrawerInt2.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.Int3:
                    return SubstanceInputDrawerInt3.DrawInput(content.DataProp, content, handler, inputID); ;

                case SubstanceValueType.Int4:
                    return SubstanceInputDrawerInt4.DrawInput(property, content, handler, inputID);

                case SubstanceValueType.Image:
                    return SubstanceInputDrawerTexture.DrawInput(content.DataProp, content, handler, inputID);

                case SubstanceValueType.String:
                    return SubstanceInputDrawerString.DrawInput(content.DataProp, content, handler, inputID);

                default:
                    return DrawDefault(property, content);
            }
        }

        private static bool DrawDefault(SerializedProperty valueProperty, SubstanceInputGUIContent content)
        {
            if (content.Description.WidgetType == SubstanceWidgetType.NoWidget)
            {
                EditorGUILayout.LabelField($"Hidden property.");
                return false;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Not supported. Value with widget {content.Description.WidgetType}");
            EditorGUI.indentLevel--;
            return false;
        }
    }
}