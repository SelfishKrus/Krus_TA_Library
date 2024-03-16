using Adobe.Substance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// Internal tools for creating and manipulating Substance objects.
    /// </summary>
    internal static class EditorTools
    {
        /// <summary>
        /// Makes an object editable. (Usefull for object managed by Importers)
        /// </summary>
        /// <param name="pObject"></param>
        public static void OverrideReadOnlyFlag(UnityEngine.Object unityObject)
        {
            unityObject.hideFlags &= ~HideFlags.NotEditable;
        }

        public class SubstanceGraphSOCreateInfo
        {
            public string Name { get; set; }
            public int Index { get; set; }

            public SubstanceGraphSOCreateInfo(string name, int index)
            {
                Name = name;
                Index = index;
            }
        }

        /// <summary>
        /// Creates an array of SubstanceGraphSO objects.
        /// </summary>
        /// <param name="assetPath">Path to the target sbsar file.</param>
        /// <param name="fileData">Sbsar file content.</param>
        /// <param name="infos">Creation info for each SubstanceGraphSO object that should be created.</param>
        public static void CreateSubstanceInstanceAsync(string assetPath, SubstanceFileRawData fileData, IEnumerable<SubstanceGraphSOCreateInfo> infos)
        {
            var instances = new List<Tuple<SubstanceGraphSO, string>>();

            foreach (var item in infos)
            {
                var instanceAsset = ScriptableObject.CreateInstance<SubstanceGraphSO>();
                instanceAsset.AssetPath = assetPath;
                instanceAsset.RawData = fileData;
                instanceAsset.Name = item.Name;
                instanceAsset.IsRoot = true;
                instanceAsset.GenerateAllMipmaps = true;
                instanceAsset.GUID = Guid.NewGuid().ToString();
                instanceAsset.Presets = new List<SubstanceEditorPreset>();

                instanceAsset.CreateGraphFolder();
                instanceAsset.SetNativeID(item.Index);

                var instancePath = Path.Combine(instanceAsset.OutputPath(), $"{instanceAsset.Name}.asset");
                SubstanceEditorEngine.instance.InitializeInstance(instanceAsset, instancePath, out SubstanceGraphSO matchingInstance);

                SubstanceEditorEngine.instance.CreateGraphObject(instanceAsset, matchingInstance);
                instances.Add(new Tuple<SubstanceGraphSO, string>(instanceAsset, instancePath));
            }

            foreach (var instance in instances)
            {
                SubstanceEditorEngine.instance.DelayAssetCreation(instance.Item1, instance.Item2);
            }
        }

        /// <summary>
        /// Create a new SubstanceGraphSO that is a copy of another SubstanceGraphSO.
        /// </summary>
        /// <param name="name">Graph name.</param>
        /// <param name="copy">Other SubstanceGraphSO to copy data from.</param>
        /// <returns>Created SubstanceGraphSO object.</returns>
        public static SubstanceGraphSO CreateSubstanceGraphCopy(string name, SubstanceGraphSO copy)
        {
            var instanceAsset = ScriptableObject.CreateInstance<SubstanceGraphSO>();
            instanceAsset.AssetPath = copy.AssetPath;
            instanceAsset.RawData = copy.RawData;
            instanceAsset.Name = name;
            instanceAsset.IsRoot = false;
            instanceAsset.GUID = Guid.NewGuid().ToString();
            instanceAsset.GenerateAllMipmaps = true;
            instanceAsset.Presets = copy.Presets;

            instanceAsset.CreateGraphFolder();
            instanceAsset.SetNativeID(copy.GetNativeID());

            var instancePath = Path.Combine(instanceAsset.OutputPath(), $"{instanceAsset.Name}.asset");
            SubstanceEditorEngine.instance.InitializeInstance(instanceAsset, instancePath, out SubstanceGraphSO _);
            SubstanceEditorEngine.instance.CreateGraphObject(instanceAsset, copy);
            AssetDatabase.CreateAsset(instanceAsset, instancePath);
            return instanceAsset;
        }

        /// <summary>
        /// Renames a given SubstanceGraphSO.
        /// </summary>
        /// <param name="substanceMaterial">Target SubstanceGraphSO.</param>
        /// <param name="name">New name.</param>
        public static void Rename(this SubstanceGraphSO substanceMaterial, string name)
        {
            var oldFolder = substanceMaterial.OutputPath();

            if (substanceMaterial.Name == name)
                return;

            substanceMaterial.Name = name;

            var dir = Path.GetDirectoryName(substanceMaterial.AssetPath);
            var assetName = Path.GetFileNameWithoutExtension(substanceMaterial.AssetPath);
            var newFolder = Path.Combine(dir, $"{assetName}_{name}");

            FileUtil.MoveFileOrDirectory(oldFolder, newFolder);
            File.Delete($"{oldFolder}.meta");

            EditorUtility.SetDirty(substanceMaterial);
            AssetDatabase.Refresh();

            var oldPath = AssetDatabase.GetAssetPath(substanceMaterial);
            var error = AssetDatabase.RenameAsset(oldPath, $"{name}.asset");

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);

            var materialOldName = AssetDatabase.GetAssetPath(substanceMaterial.OutputMaterial);
            var materialNewName = Path.GetFileName(substanceMaterial.GetAssociatedAssetPath($"{name}_material", "mat"));
            error = AssetDatabase.RenameAsset(materialOldName, materialNewName);
            EditorUtility.SetDirty(substanceMaterial.OutputMaterial);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Moves a SubstanceGraphSO asset and its associated assets to a new location.
        /// </summary>
        /// <param name="substanceMaterial">Target SubstanceGraphSO</param>
        /// <param name="to">New location path.</param>
        public static void Move(this SubstanceGraphSO substanceMaterial, string to)
        {
            var pathTO = Path.GetDirectoryName(to);

            var oldMaterialPath = AssetDatabase.GetAssetPath(substanceMaterial.OutputMaterial);
            AssetDatabase.MoveAsset(oldMaterialPath, Path.Combine(pathTO, Path.GetFileName(oldMaterialPath)));

            foreach (var output in substanceMaterial.Output)
            {
                var textureAssetPath = AssetDatabase.GetAssetPath(output.OutputTexture);
                var textureFileName = Path.GetFileName(textureAssetPath);
                var newTexturePath = Path.Combine(pathTO, textureFileName);
                AssetDatabase.MoveAsset(textureAssetPath, newTexturePath);
            }

            EditorUtility.SetDirty(substanceMaterial);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Returns the path for a asset associated with this SubstanceGraphSO. (With naming convention applied.)
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="name">Asset name</param>
        /// <param name="extension">Asset extension</param>
        /// <returns>Asset path.</returns>
        public static string GetAssociatedAssetPath(this SubstanceGraphSO graph, string name, string extension)
        {
            var outputPath = graph.OutputPath();

            var fileName = Path.GetFileNameWithoutExtension(graph.AssetPath);
            return Path.Combine(outputPath, $"{fileName}_{name}.{extension}");
        }

        /// <summary>
        /// Creates a new folder to store assets associated with a SubstanceGraphSO;
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        private static void CreateGraphFolder(this SubstanceGraphSO graphSO)
        {
            var outputPath = graphSO.OutputPath();

            if (!Directory.Exists(outputPath))
            {
                var folderPath = Path.GetDirectoryName(outputPath);
                var folderName = Path.GetFileNameWithoutExtension(outputPath);
                AssetDatabase.CreateFolder(folderPath, folderName);
            }
        }

        static readonly char[] extraInvalidChars = { '.' };

        private static string ReplaceInvalidChars(string filename)
        {
            foreach (char c in Path.GetInvalidPathChars())
            {
                filename = filename.Replace(c, '_');
            }

            foreach (char c in extraInvalidChars)
            {
                filename = filename.Replace(c, '_');
            }

            return filename;
        }

        /// <summary>
        /// Returns the output path for assets associated with a given graph.
        /// </summary>
        /// <param name="graphSO">Target graphSO</param>
        /// <returns>Assets output folder.</returns>
        internal static string OutputPath(this SubstanceGraphSO graphSO)
        {
            var graphPath = AssetDatabase.GetAssetPath(graphSO);

            if (!string.IsNullOrEmpty(graphPath))
            {
                return Path.GetDirectoryName(graphPath);
            }

            var dir = Path.GetDirectoryName(graphSO.AssetPath);
            var sbsarName = Path.GetFileNameWithoutExtension(graphSO.AssetPath);
            sbsarName = ReplaceInvalidChars(sbsarName);
            return Path.Combine(dir, $"{sbsarName}_{graphSO.Name}");
        }

        internal static void GetShaderProperties(Shader shader, List<string> result)
        {
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    result.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
        }

        internal static List<string> GetShaderProperties(Shader shader)
        {
            List<string> result = new List<string>();
            GetShaderProperties(shader, result);
            return result;
        }

        internal static bool IsStandardOutput(this SubstanceOutputTexture output, List<string> shaderProperties)
        {
            if (string.IsNullOrEmpty(output.MaterialTextureTarget))
                return false;

            return shaderProperties.FindIndex(0, shaderProperties.Count, (value) => { return output.MaterialTextureTarget.Equals(value, StringComparison.OrdinalIgnoreCase); }) >= 0;
        }

    }
}