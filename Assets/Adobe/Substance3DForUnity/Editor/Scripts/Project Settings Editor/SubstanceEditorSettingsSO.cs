using Adobe.Substance;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor.ProjectSettings
{
    /// <summary>
    /// Global editor settings scriptable object
    /// </summary>
    internal class SubstanceEditorSettingsSO : ScriptableObject
    {
        private static string _editorSettingsAsset => $"{PathUtils.SubstanceRootPath}/Editor/Settings/SubstanceEditorSettings.asset";

        [SerializeField]
        private bool _generateAllTexture;

        [SerializeField]
        private Vector2Int _targetResolution;

        internal static SubstanceEditorSettingsSO GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SubstanceEditorSettingsSO>(_editorSettingsAsset);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SubstanceEditorSettingsSO>();
                settings._generateAllTexture = false;
                settings._targetResolution = new Vector2Int(9, 9);
                AssetDatabase.CreateAsset(settings, _editorSettingsAsset);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        public static Vector2Int TextureOutputResultion()
        {
            var settigns = GetOrCreateSettings();
            return settigns._targetResolution;
        }

        public static bool GenerateAllTextures()
        {
            var settigns = GetOrCreateSettings();
            return settigns._generateAllTexture;
        }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(_editorSettingsAsset);
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}