using Adobe.Substance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceGraphSOExtensions
    {
        /// <summary>
        /// Updates SubstanceGraphSO related assets using the renderResult data.
        /// </summary>
        /// <param name="renderResult">Target render result.</param>
        /// <param name="substance">Owner substance.</param>
        /// <returns>Returns true if textures must be reassigned to the material.</returns>
        internal static bool UpdateAssociatedAssets(this SubstanceGraphSO graph, IntPtr renderResult, bool forceRebuild)
        {
            if (!forceRebuild && CheckIfTextureAssetsExist(graph))
            {
                ResizeExistingTexturesIfNeeded(renderResult, graph);
                graph.UpdateOutputTextures(renderResult);
                return false;
            }

            graph.CreateAndUpdateOutputTextures(renderResult);

            if (graph.IsRuntimeOnly)
                return true;

            graph.SaveOutputsTGAFiles();
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// Updates TGA files using the currect values of the outputs Texture2D.
        /// </summary>
        /// <param name="graph"></param>
        private static void SaveOutputsTGAFiles(this SubstanceGraphSO graph)
        {
            foreach (var substanceOutput in graph.Output)
            {
                var texture = substanceOutput.OutputTexture;

                if (texture == null)
                    continue;

                var textureOutput = graph.GetAssociatedAssetPath(substanceOutput.Description.Identifier, "tga");
                var bytes = texture.EncodeToTGA();
                File.WriteAllBytes(textureOutput, bytes);
            }

            AssetDatabase.Refresh();

            graph.ConfigureTextureImporter();
        }

        /// <summary>
        /// Properly configure TextureImporters settings based on output types.
        /// </summary>
        /// <param name="graph"></param>
        private static void ConfigureTextureImporter(this SubstanceGraphSO graph)
        {
            foreach (var substanceOutput in graph.Output)
            {
                var texture = substanceOutput.OutputTexture;

                if (texture == null)
                    continue;

                var textureOutput = graph.GetAssociatedAssetPath(substanceOutput.Description.Identifier, "tga");
                substanceOutput.OutputTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureOutput);
                ConfigureTextureImporter(substanceOutput);
            }
        }

        /// <summary>
        /// Resizes existing texture to match the expected values from renderResult.
        /// </summary>
        /// <param name="renderResult">Render result.</param>
        /// <param name="graph">Target graph.</param>
        private static void ResizeExistingTexturesIfNeeded(IntPtr renderResult, SubstanceGraphSO graph)
        {
            var renderResultsSizes = graph.GetResizedOutputs(renderResult);

            //Resize existing output textures.
            if (renderResultsSizes.Count != 0)
            {
                foreach (var resultSize in renderResultsSizes)
                {
                    var outputIndex = resultSize.Item1;
                    var outputSize = resultSize.Item2;
                    var targetOutput = graph.Output.FirstOrDefault(a => a.Index == outputIndex);

#if UNITY_2021_2_OR_NEWER
                    targetOutput.OutputTexture.Reinitialize(outputSize.x, outputSize.y);
#else
                    targetOutput.OutputTexture.Resize(outputSize.x, outputSize.y);
#endif
                    if (!graph.IsRuntimeOnly)
                    {
                        var bytes = targetOutput.OutputTexture.EncodeToTGA();
                        var assetPath = AssetDatabase.GetAssetPath(targetOutput.OutputTexture);
                        File.WriteAllBytes(assetPath, bytes);
                    }
                }

                AssetDatabase.Refresh();
            }
        }     

        /// <summary>
        /// Try to get the texture2D instances for a give graph.
        /// </summary>
        /// <param name="graph">Target graph.</param>
        /// <param name="textures">Array of texture2D instances attached to each substance output.</param>
        /// <returns>True if all textures instances exists. If false they must be rebuild.</returns>
        private static bool CheckIfTextureAssetsExist(SubstanceGraphSO graph)
        {
            var shaderProperties = EditorTools.GetShaderProperties(graph.GetShader());

            foreach (var output in graph.Output)
            {
                if (!output.IsStandardOutput(shaderProperties) && !graph.GenerateAllOutputs)
                {
                    if (output.OutputTexture != null)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(output.OutputTexture);

                        if (!string.IsNullOrEmpty(assetPath))
                            AssetDatabase.DeleteAsset(assetPath);

                        output.OutputTexture = null;
                    }

                    continue;
                }

                if (output.OutputTexture == null)
                    return false;

                output.OutputTexture = TextureUtils.EnsureTextureCorrectness(output.OutputTexture, !output.IsNormalMap(), graph.GenerateAllMipmaps);
            }

            return true;
        }

        /// <summary>
        /// Configures the texture importer settings to the associated texture output.
        /// </summary>
        /// <param name="textureOutput">Target output texture.</param>
        private static void ConfigureTextureImporter(SubstanceOutputTexture textureOutput)
        {
            var texturePath = AssetDatabase.GetAssetPath(textureOutput.OutputTexture);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (importer == null)
                return;

            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            importer.maxTextureSize = 4096;
            importer.sRGBTexture = textureOutput.sRGB;

            if (textureOutput.IsNormalMap())
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                var defaultSettings = importer.GetDefaultPlatformTextureSettings();

                if (defaultSettings.format != TextureImporterFormat.RGBA32)
                {
                    defaultSettings.format = TextureImporterFormat.RGBA32;
                    importer.SetPlatformTextureSettings(defaultSettings);
                }
            }

            EditorUtility.SetDirty(importer);
            AssetDatabase.WriteImportSettingsIfDirty(texturePath);
        }
    }
}