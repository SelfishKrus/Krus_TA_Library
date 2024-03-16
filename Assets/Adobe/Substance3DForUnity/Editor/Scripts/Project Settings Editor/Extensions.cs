using Adobe.Substance;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor.ProjectSettings
{
    internal static class Extensions
    {
        public static void DrawAboutWindow()
        {
            string aboutMessage = "Plugin Info:\n";
            aboutMessage += "Package Name: Substance 3D for Unity" + "\n";
            aboutMessage += "Package Version: " + Version.PluginVersion + "\n";
            aboutMessage += "Engine Version: " + Version.EngineVersion + "\n";

            bool state = EditorUtility.DisplayDialog("About Substance 3D", aboutMessage, "Copy to clipboard", "Close");

            if (state == true)
            {
                // Copy to clipboard:
                TextEditor _textEditor = new TextEditor();
                _textEditor.text = aboutMessage;
                _textEditor.OnFocus();
                _textEditor.Copy();
                DrawAboutWindow();
            }
        }
    }
}