using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Adobe.Substance
{
    public static class MaterialUtils
    {
        /// <summary>
        /// Default shader for the HDRP pipeline.
        /// </summary>
        private const string HDRPShaderName = "HDRP/Lit";

        /// <summary>
        /// Default shader for the URP pipeline.
        /// </summary>
        private const string URPShaderName = "Universal Render Pipeline/Lit";

        /// <summary>
        /// Default shader for the Standard unity render pipeline.
        /// </summary>
        private const string StandardShaderName = "Standard";

        /// <summary>
        /// Table for converting substance output names to textures inputs in HDRP shaders.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> HDRPOutputTable = new Dictionary<string, string>()
        {
            { "basecolor", "_BaseColorMap"},
            { "diffuse", "_BaseColorMap" },
            { "mask", "_MaskMap" },
            { "normal", "_NormalMap" },
            { "height", "_HeightMap" },
            { "emissive", "_EmissiveColorMap" },
            { "specular", "_SpecularColorMap" },
            { "detailMask", "_DetailMask" },
            { "opacity", "_OpacityMap" },
            { "glossiness", "_GlossinessMap" },
            { "ambientocclusion", "_OcclusionMap" },
            { "metallic", "_MetallicGlossMap" },
            { "roughness", "_RoughnessMap" }
        };

        /// <summary>
        /// Table for converting substance output names to textures inputs in URP shaders.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> URPOutputTable = new Dictionary<string, string>()
        {
            { "basecolor" , "_BaseMap" },
            { "diffuse", "_BaseMap" },
            { "normal" , "_BumpMap" },
            { "height" ,"_ParallaxMap" },
            { "emissive" , "_EmissionMap" },
            { "specular" , "_SpecGlossMap" },
            { "ambientocclusion" , "_OcclusionMap" },
            { "metallic" , "_MetallicGlossMap" },
            { "mask" , "_MaskMap" },
            { "detailMask" , "_DetailMask" },
            { "opacity" ,"_OpacityMap" },
            { "glossiness" ,"_GlossinessMap" },
            { "roughness" ,"_RoughnessMap" }
        };

        /// <summary>
        /// Table for converting substance output names to textures inputs in unity Standard pipeline shaders.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> StandardOutputTable = new Dictionary<string, string>()
        {
            { "basecolor", "_MainTex" },
            { "diffuse", "_MainTex" },
            { "normal" , "_BumpMap" },
            { "height" ,"_ParallaxMap" },
            { "emissive" ,"_EmissionMap" },
            { "specular" ,"_SpecGlossMap" },
            { "specularlevel" ,"_SpecularLevelMap" },
            { "opacity", "_OpacityMap" },
            { "glossiness" ,"_GlossinessMap" },
            { "ambientocclusion" ,"_OcclusionMap" },
            { "detailmask" ,"_DetailMask" },
            { "metallic" ,"_MetallicGlossMap" },
            { "roughness" ,"_RoughnessMap" }
        };

        private static IReadOnlyDictionary<string, string> GetTextureMappingDictionary()
        {
            if (PluginPipelines.IsHDRP())
                return HDRPOutputTable;

            if (PluginPipelines.IsURP())
                return URPOutputTable; // for now

            return StandardOutputTable;
        }

        public static void AssignOutputTexturesToMaterial(SubstanceGraphSO graph)
        {
            foreach (var output in graph.Output)
            {
                if (output.OutputTexture == null)
                    continue;

                Texture2D texture = output.OutputTexture;
                var shaderTextureName = output.MaterialTextureTarget;
                EnableShaderKeywords(graph.OutputMaterial, shaderTextureName);
                graph.OutputMaterial.SetTexture(shaderTextureName, texture);
            }

            var smoothnessChannel = GetSmoothnessChannelAssignment(graph);
            graph.OutputMaterial.SetInt("_SmoothnessTextureChannel", smoothnessChannel);
            graph.OutputMaterial.SetFloat("_Glossiness", 1.0f);
            graph.OutputMaterial.SetFloat("_Smoothness", 1.0f);
            graph.OutputMaterial.SetFloat("_OcclusionStrength", 1.0f);

            var opacityOutput = graph.Output.FirstOrDefault(a => a.IsOpacity());
        }

        public static string GetUnityTextureName(SubstanceOutputDescription description)
        {
            var dictionary = GetTextureMappingDictionary();

            if (dictionary.TryGetValue(description.Channel.ToLower(), out string result))
                return result;

            return string.Empty;
        }

        private static void EnableShaderKeywords(Material material, string shaderTextureName)
        {
            if (shaderTextureName == "_BumpMap")
            {
                material.EnableKeyword("_NORMALMAP");
            }
            else if (shaderTextureName == "_EmissionMap")
            {
                // Enables emission for the material, and make the material use
                // realtime emission.
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                // Update the emission color and intensity of the material.
                material.SetColor("_EmissionColor", Color.white);

                // Inform Unity's GI system to recalculate GI based on the new emission map.
                DynamicGI.UpdateEnvironment();
            }
            else if (shaderTextureName == "_ParallaxMap")
            {
                material.EnableKeyword("_PARALLAXMAP");
            }
            else if (shaderTextureName == "_MetallicGlossMap")
            {
                if (PluginPipelines.IsURP())
                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                else
                    material.EnableKeyword("_METALLICGLOSSMAP");
            }
        }

        /// <summary>
        /// Returns 1 if smoothness is assigned to _MainTex alpha channel and 0 if it is assigned to metallic map alpha channel.
        /// </summary>
        /// <param name="graph">Target substance graph.</param>
        /// <returns>0 or 1 depending on the smoothness channel assignment. </returns>
        private static int GetSmoothnessChannelAssignment(SubstanceGraphSO graph)
        {
            var baseColorOutput = graph.Output.FirstOrDefault(a => a.IsBaseColor());

            //Check if smoothness is assigned to baseColor.
            if (baseColorOutput != null)
                if (baseColorOutput.AlphaChannel == "roughness" || baseColorOutput.AlphaChannel == "glossiness")
                    return 1;

            //Check if smoothness is assigned to diffuse.
            var diffuseOutput = graph.Output.FirstOrDefault(a => a.IsDiffuse());

            if (diffuseOutput != null)
                if (diffuseOutput.AlphaChannel == "roughness" || diffuseOutput.AlphaChannel == "glossiness")
                    return 1;

            //Assumes it is assinged to metallic map.
            return 0;
        }

        public static bool CheckIfStandardOutput(SubstanceOutputDescription description)
        {
            if (PluginPipelines.IsHDRP())
            {
                return CheckIfHRPStandardOutput(description);
            }
            else if (PluginPipelines.IsURP())
            {
                return CheckIfURPStandardOutput(description);
            }

            //Unity Standard render pipeline.
            return CheckIfStandardPipelineOutput(description);
        }

        private static bool CheckIfURPStandardOutput(SubstanceOutputDescription description)
        {
            if (description == null)
                return false;

            if (string.IsNullOrEmpty(description.Channel))
                return false;

            if (string.Equals(description.Channel, "baseColor", StringComparison.OrdinalIgnoreCase)
                || string.Equals(description.Channel, "diffuse", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!URPOutputTable.TryGetValue(description.Channel, out string shaderValue))
                return false;

            var material = new Material(GetStandardShader());
            return material.HasProperty(shaderValue);
        }

        private static bool CheckIfHRPStandardOutput(SubstanceOutputDescription description)
        {
            if (description == null)
                return false;

            if (string.IsNullOrEmpty(description.Channel))
                return false;

            switch (description.Channel)
            {
                case "baseColor":
                    return true;

                case "normal":
                    return true;

                case "mask":
                    return true;

                case "height":
                    return true;

                case "emissive":
                    return true;

                default:
                    return false;
            }
        }

        private static bool CheckIfStandardPipelineOutput(SubstanceOutputDescription description)
        {
            if (description == null)
                return false;

            if (string.IsNullOrEmpty(description.Channel))
                return false;

            var channel = description.Channel.ToLower();

            if ("basecolor" == channel)
                return true;

            if (!StandardOutputTable.TryGetValue(channel, out string shaderValue))
                return false;

            var material = new Material(GetStandardShader());
            return material.HasProperty(shaderValue);
        }

        public static Shader GetStandardShader()
        {
            if (PluginPipelines.IsHDRP())
                return Shader.Find(HDRPShaderName);
            else if (PluginPipelines.IsURP())
                return Shader.Find(URPShaderName);

            return Shader.Find(StandardShaderName);
        }

        public static bool CheckIfBGRA(SubstanceOutputDescription description)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor
                    || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.OSXEditor
                    || Application.platform == RuntimePlatform.OSXPlayer)
            {
                return false;
            }

            switch (description.Channel)
            {
                case "baseColor":
                case "diffuse":
                case "emissive":
                case "normal":
                    return true;

                default:
                    return false;
            }
        }
     
        #region PhysicalSize

        public static void ApplyPhysicalSize(Material material, Vector3 physicalSize, bool enablePhysicalSize)
        {
            if (PluginPipelines.IsHDRP())
                ApplyPhysicalSizeHDRP(material, physicalSize, enablePhysicalSize);
        }

        public static Vector3 GetPhysicalSizePositionOffset(Material material)
        {
            if (PluginPipelines.IsHDRP())
                return GetPhysicalSizePositionOffsetHDRP(material);

            return new Vector3(0, 0, 0);
        }

        public static void SetPhysicalSizePositionOffset(Material material, Vector3 offset)
        {
            if (PluginPipelines.IsHDRP())
                SetPhysicalSizePositionOffsetHDRP(material, offset);
        }

        private static void ApplyPhysicalSizeHDRP(Material material, Vector3 physicalSize, bool enablePhysicalSize)
        {
            if (enablePhysicalSize)
            {
                material.SetFloat("_UVBase", 5);
                material.SetFloat("_UVEmissive", 5);
                material.SetFloat("_TexWorldScale", 100);
                material.EnableKeyword("_MAPPING_PLANAR");
                material.EnableKeyword("_EMISSIVE_MAPPING_TRIPLANAR");
                material.mainTextureScale = new Vector2(1f / physicalSize.x, 1f / physicalSize.y);
            }
            else
            {
                material.SetFloat("_UVBase", 0);
                material.SetFloat("_UVEmissive", 0);
                material.SetFloat("_TexWorldScale", 1);
                material.DisableKeyword("_MAPPING_PLANAR");
                material.DisableKeyword("_EMISSIVE_MAPPING_TRIPLANAR");
                material.mainTextureScale = new Vector2(1, 1);
            }
        }

        public static Vector3 GetPhysicalSizePositionOffsetHDRP(Material material)
        {
            return material.GetTextureOffset("_BaseColorMap");
        }

        public static void SetPhysicalSizePositionOffsetHDRP(Material material, Vector3 offset)
        {
            material.SetTextureOffset("_BaseColorMap", offset);
            material.SetTextureOffset("_EmissiveColorMap", offset);
        }

        #endregion PhysicalSize

        #region Live output assignment

        public static void UpdateTextureTarget(Material material, Texture2D texture, string oldstring, string newString)
        {
            material.SetTexture(oldstring, null);

            if (!string.IsNullOrEmpty(newString))
                material.SetTexture(newString, texture);
        }

        public static void EnableEmissionIfAssigned(Material material)
        {
            var emissionTextureName = GetTextureMappingDictionary()["emissive"];

            if (material.GetTexture(emissionTextureName) != null)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                material.SetColor("_EmissionColor", Color.white);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                material.SetColor("_EmissionColor", Color.black);
            }
        }

        #endregion Live output assignment

        static public Shader GetShader(this SubstanceGraphSO graph)
        {
            return (graph.OutputMaterial != null) ? graph.OutputMaterial.shader : GetStandardShader();
        }
    }
}