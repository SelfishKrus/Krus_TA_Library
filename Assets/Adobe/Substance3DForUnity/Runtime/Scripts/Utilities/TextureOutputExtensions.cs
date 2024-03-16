using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance
{
    public static class TextureOutputExtensions
    {
        public static bool IsDiffuse(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "diffuse");
        }

        public static bool IsNormalMap(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "normal");
        }

        public static bool IsBaseColor(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "basecolor");
        }

        public static bool IsRoughness(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "roughness");
        }

        public static bool IsOpacity(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "opacity");
        }

        public static bool IsHightMap(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "height");
        }

        public static bool IsMetallicness(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "metallic");
        }

        public static bool IsSpecular(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "specular");
        }

        public static bool IsOcclusion(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "ambientocclusion");
        }

        public static bool IsEmissive(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "emissive");
        }

        public static bool IsDetail(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "detailmask");
        }

        public static bool IsHDRPMask(this SubstanceOutputTexture output)
        {
            return CheckChannelName(output, "mask");
        }

        private static bool CheckChannelName(SubstanceOutputTexture output, string channelName)
        {
            var label = output.Description.Channel;
            return string.Equals(label, channelName, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsStandardOutput(this SubstanceOutputTexture output, Material material)
        {
            if (string.IsNullOrEmpty(output.MaterialTextureTarget))
                return false;

            if (material == null)
                return output.IsStandardOutput();

#if UNITY_2021_1_OR_NEWER
            return material.HasTexture(output.MaterialTextureTarget);
#else
            return material.HasProperty(output.MaterialTextureTarget);
#endif
        }       

        public static bool IsStandardOutput(this SubstanceOutputTexture output)
        {
            return MaterialUtils.CheckIfStandardOutput(output.Description);
        }
    }
}