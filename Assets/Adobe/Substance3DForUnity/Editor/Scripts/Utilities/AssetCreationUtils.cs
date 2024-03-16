using Adobe.Substance;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    /// <summary>
    /// General utilites for material creating and output texture assignment.
    /// </summary>
    internal static class AssetCreationUtils
    {
        /// <summary>
        /// Creates a Unity material and set its textures according to the currently in use Unity render pipeline.//
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static void CreateMaterialOrUpdateMaterial(SubstanceGraphSO graph, string instanceName)
        {
            var materialOutput = graph.GetAssociatedAssetPath($"{instanceName}_material", "mat");
            var oldMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialOutput);

            if (oldMaterial != null)
            {
                graph.OutputMaterial = oldMaterial;
            }

            bool createMaterial = graph.OutputMaterial == null;

            if (createMaterial)
            {
                graph.OutputMaterial = new Material(MaterialUtils.GetStandardShader())
                {
                    name = Path.GetFileNameWithoutExtension(materialOutput)
                };
            }

            MaterialUtils.AssignOutputTexturesToMaterial(graph);

            if (createMaterial)
                AssetDatabase.CreateAsset(graph.OutputMaterial, materialOutput);
            else
                EditorUtility.SetDirty(graph.OutputMaterial);

            graph.MaterialShader = graph.OutputMaterial.shader.name;
        }

        public static void UpdateMeterialAssignment(SubstanceGraphSO graph)
        {
            graph.MaterialShader = graph.OutputMaterial.shader.name;
            var shaderProperties = EditorTools.GetShaderProperties(graph.GetShader());

            foreach (var output in graph.Output)
            {
                if (!output.IsStandardOutput(shaderProperties) && (!graph.GenerateAllOutputs))
                {
                    var texturePath = AssetDatabase.GetAssetPath(output.OutputTexture);

                    if (File.Exists(texturePath))
                        AssetDatabase.DeleteAsset(texturePath);

                    output.OutputTexture = null;
                }
            }
        }
    }
}