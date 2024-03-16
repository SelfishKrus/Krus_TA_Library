using Adobe.Substance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Adobe.SubstanceEditor.ProjectSettings
{
    /// <summary>
    /// Settings provider for the Adobe Substance tab in the Unity project settings UI.
    /// </summary>
    internal class SubstanceEditorSettingsProvider : SettingsProvider
    {
        private const string substanceURL = "https://substance3d.adobe.com/assets/";

        private SerializedObject _editorSettings;

        private SerializedProperty _generateAllTextureProp;

        private SerializedProperty _targetResolutionProp;

        private static string SubstanceAssetLogoPath => $"{PathUtils.SubstanceRootPath}/Editor/Assets/S_3DHeart_18_N_nudged.png";
        private static string SubstanceCommunityAssetLogoPath => $"{PathUtils.SubstanceRootPath}/Editor/Assets/S_3DCummunityAssets_18_N.png";

        private const string SubstanceCommunityURL = "https://substance3d.adobe.com/community-assets";

        private const string SubstanceAssetURL = "https://substance3d.adobe.com/assets";

        private class Contents
        {
            public static readonly GUIContent GenerateAllTexturesText = new GUIContent("Generate all outputs", "Generate all output textures for the substance graphs.");
            public static readonly GUIContent TextureResoltuionText = new GUIContent("Texture resolution", "Texture resolution for all graphs outputs.");
            public static readonly GUIContent SubstanceAssetsIcon = new GUIContent();
            public static readonly GUIContent SubstanceCommunityIcon = new GUIContent();
        }

        private class Styles
        {
            public static GUIStyle AssetButtonsStyle;
            public static readonly GUIStyle SubstanceAssetButtonsPanelStyle = new GUIStyle();
            public static readonly GUIStyle RichTextStyle = new GUIStyle() { richText = true };
        }

        public SubstanceEditorSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _editorSettings = SubstanceEditorSettingsSO.GetSerializedSettings();
            _generateAllTextureProp = _editorSettings.FindProperty("_generateAllTexture");
            _targetResolutionProp = _editorSettings.FindProperty("_targetResolution");

            Contents.SubstanceAssetsIcon.image = AssetDatabase.LoadAssetAtPath<Texture2D>(SubstanceAssetLogoPath);
            Contents.SubstanceAssetsIcon.tooltip = SubstanceAssetURL;

            Contents.SubstanceCommunityIcon.image = AssetDatabase.LoadAssetAtPath<Texture2D>(SubstanceCommunityAssetLogoPath);
            Contents.SubstanceCommunityIcon.tooltip = SubstanceCommunityURL;
        }

        public override void OnGUI(string searchContext)
        {
            _editorSettings.Update();

            if (Styles.AssetButtonsStyle == null)
            {
                Styles.AssetButtonsStyle = new GUIStyle(GUI.skin.label);
                Styles.AssetButtonsStyle.fixedHeight = 24;
                Styles.AssetButtonsStyle.fixedWidth = 24;
            }

            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            {
                if (_generateAllTextureProp != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(_generateAllTextureProp, Contents.GenerateAllTexturesText, GUILayout.Width(100));
                }

                if (_targetResolutionProp != null)
                {
                    EditorGUILayout.Space();
                    EditorDrawUtilities.DrawResolutionSelection(_targetResolutionProp, Contents.TextureResoltuionText);
                }

                DrawEngineInfo();

                EditorGUILayout.Space();

                DrawTextLinksAndAbout();
                //DrawAboutText();
            }
            EditorGUI.indentLevel--;

            _editorSettings.ApplyModifiedProperties();
        }

        private void DrawEngineInfo()
        {
            string label = PlatformUtils.IsCPU() ? "CPU" : "GPU";
            var content = new GUIContent($"Computing textures with {label}", "Engine used for rendering textures");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a hyperlink label that should redirect the user to a URL.
        /// </summary>
        /// <param name="text">Label text.</param>
        /// <param name="url">Redirect URL.</param>
        private void DrawClickableText(string text, GUIStyle style, Action callback)
        {
            var labelRect = EditorGUILayout.GetControlRect();

            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
                callback();

            GUI.Label(labelRect, text, style);
        }

        private void DrawTextLinksAndAbout()
        {
            var textStyle = new GUIStyle();
            textStyle.normal.textColor = new Color(75f / 255f, 122f / 255f, 243f / 255f);
            textStyle.alignment = TextAnchor.LowerLeft;
            textStyle.fixedWidth = 150;

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(10, false);

                if (GUILayout.Button(Contents.SubstanceAssetsIcon, Styles.AssetButtonsStyle))
                    Application.OpenURL(SubstanceAssetURL);

                DrawClickableText("Substance 3D assets", textStyle, () => Application.OpenURL(SubstanceAssetURL));
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(10, false);

                if (GUILayout.Button(Contents.SubstanceCommunityIcon, Styles.AssetButtonsStyle))
                    Application.OpenURL(SubstanceCommunityURL);

                DrawClickableText("Substance 3D community assets", textStyle, () => Application.OpenURL(SubstanceCommunityURL));
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(40, false);
                DrawClickableText("About", textStyle, () => Extensions.DrawAboutWindow());
            }
            GUILayout.EndHorizontal();
        }

        #region Registration

        [SettingsProvider]
        public static SettingsProvider CreateSubstanceSettingsProvider()
        {
            if (SubstanceEditorSettingsSO.IsSettingsAvailable())
            {
                return new SubstanceEditorSettingsProvider("Project/Adobe Substance 3D", SettingsScope.Project)
                {
                    label = "Adobe Substance 3D",
                    keywords = GetSearchKeywordsFromGUIContentProperties<Contents>()
                };
            }

            return null;
        }

        #endregion Registration
    }
}