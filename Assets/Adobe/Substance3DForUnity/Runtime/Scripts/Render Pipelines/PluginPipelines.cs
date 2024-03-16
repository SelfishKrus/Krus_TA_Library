using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance
{
    public class PluginPipelines
    {
        private static Pipeline current = Pipeline.UNKNOWN;
        private static string currentText = "UNKNOWN";

        public static Pipeline GetCurrent()
        {
            return current;
        }

        public static string GetCurrentText()
        {
            return currentText;
        }

        public static void SetCurrent(Pipeline pSetting)
        {
            switch (pSetting)
            {
                case Pipeline.HDRP:
                    current = Pipeline.HDRP;
                    currentText = "HDRP";
                    break;

                case Pipeline.URP:
                    current = Pipeline.URP;
                    currentText = "URP";
                    break;

                default:
                    current = Pipeline.DEFAULT;
                    currentText = "DEFAULT";
                    break;
            }
        }

        public static bool IsUNKNOWN()
        {
            return (current == Pipeline.UNKNOWN);
        }

        public static bool IsDEFAULT()
        {
            return (current == Pipeline.DEFAULT);
        }

        public static bool IsHDRP()
        {
            return (current == Pipeline.HDRP);
        }

        public static bool IsURP()
        {
            return (current == Pipeline.URP);
        }

        public static void GetCurrentPipelineInUse()
        {
            if (IsUNKNOWN())
            {
                if (UnityPipeline.IsHDRP())
                    SetCurrent(Pipeline.HDRP);
                else if (UnityPipeline.IsURP())
                    SetCurrent(Pipeline.URP);
                else
                    SetCurrent(Pipeline.DEFAULT);
            }
        }

        private static class UnityPipeline
        {
            public static bool IsHDRP()
            {
#if UNITY_2019_3_OR_NEWER
                bool bActive = false;

                UnityEngine.Rendering.RenderPipelineAsset asset;
                asset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

                if ((asset != null) &&
                    (asset.GetType().ToString().EndsWith(".HDRenderPipelineAsset")))
                {
                    bActive = true;
                }

                return bActive;
#else
                return false;
#endif
            }

            public static bool IsURP()
            {
#if UNITY_2019_3_OR_NEWER
                bool bActive = false;

                UnityEngine.Rendering.RenderPipelineAsset asset;
                asset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

                if ((asset != null) &&
                    (asset.GetType().ToString().EndsWith("UniversalRenderPipelineAsset")))
                {
                    bActive = true;
                }

                return bActive;
#else
                return false;
#endif
            }
        }

        public enum Pipeline
        {
            UNKNOWN = -1,
            DEFAULT = 0,
            HDRP = 1,
            URP = 2
        }
    }
}