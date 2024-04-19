using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SJSJ.BattleRendererApplication.RenderPipeline {


    // Post Processing Renderer Feature Settings /////////////////////////////////////////////////////////
    // to control the common settings of renderer feature shared among different render passes
    [Serializable]
    public class PostProcessingRendererFeatureSetting {

        [Range(1, 8)]
        public int downsample = 1; // downsample rate of render target
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents; // queue

    }

    // Post Processing Renderer Feature //////////////////////////////////////////////////////////////////
    public class PostProcessingRendererFeature : ScriptableRendererFeature {

        // Common Renderer Feature Settings // 
        [SerializeField] PostProcessingRendererFeatureSetting GlobalSettings;

        // Brightness // 
        [SerializeField] BrightnessPPSettings brightnessSettings;
        BrightnessPPRenderPass brightnessPPRenderPass;
        public BrightnessPPRenderPass BrightnessPPRenderPass => brightnessPPRenderPass;

        // Radial Blur // 
        [SerializeField] RadialBlurPPSettings radialBlurSettings;
        RadialBlurPPRenderPass radialBlurPPRenderPass;
        public RadialBlurPPRenderPass RadialBlurPPRenderPass => radialBlurPPRenderPass;

        // Radial Distortion // 
        [SerializeField] RadialDistortionPPSettings radialDistortionSettings;
        RadialDistortionPPRenderPass radialDistortionPPRenderPass;

        // === Initialization of Render Feature; Pass UI Settings to Render Pass === //
        public override void Create() {

            // Brightness // 
            brightnessPPRenderPass = new BrightnessPPRenderPass(GlobalSettings, brightnessSettings);

            // Radial Blur // 
            radialBlurPPRenderPass = new RadialBlurPPRenderPass(GlobalSettings, radialBlurSettings);

            // Radial Distortion //
            radialDistortionPPRenderPass = new RadialDistortionPPRenderPass(GlobalSettings, radialDistortionSettings);

        }

        public void Initialize() {
            brightnessPPRenderPass.Initialize();
            radialBlurPPRenderPass.Initialize();
        }

        // === Set render pass order === //
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {

            renderer.EnqueuePass(brightnessPPRenderPass);
            renderer.EnqueuePass(radialBlurPPRenderPass);
            renderer.EnqueuePass(radialDistortionPPRenderPass);

        }

    }

}