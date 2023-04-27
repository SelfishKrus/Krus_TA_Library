using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SJSJ.BattleRendererApplication.RenderPipeline {

    public class BrightnessPPRenderPass : ScriptableRenderPass {

        PostProcessingRendererFeatureSetting rfSettings;

        BrightnessPPSettings ppSettings;

        bool isEnabled;

        Material mat;

        int tmpId1;
        int tmpId2;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;

        RenderTargetIdentifier cameraColorTexture;

        public BrightnessPPRenderPass(PostProcessingRendererFeatureSetting RFsettings, BrightnessPPSettings PPsettings) {

            this.ppSettings = PPsettings;
            this.renderPassEvent = RFsettings.renderPassEvent;
            this.rfSettings = RFsettings;

            this.isEnabled = ppSettings.isEnabled;

            Initialize();

            mat.SetFloat("_Brightness", ppSettings._Brightness);

        }

        // FUNCTION to set values during runtime //////////////////////////////////////////////////////////
        public void Initialize() {
            mat = new Material(ppSettings.mat);
        }

        public void SetBrightness(float value) {
            mat.SetFloat("_Brightness", value);
        }

        public void SetEnabled(bool value) {
            this.isEnabled = value;
        }

        // To Set Render Target //////////////////////////////////////////////////////////////////////////
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {

            // instantiate a new material 
            if (mat == null) {
                return;
            }
            // INITIALIZATION : pass values to new mat

            int downsample = rfSettings.downsample;
            var width = cameraTextureDescriptor.width / downsample;
            var height = cameraTextureDescriptor.height / downsample;

            // Create temporary render target
            tmpId1 = Shader.PropertyToID("tmpBlurRT1");
            tmpId2 = Shader.PropertyToID("tmpBlurRT2");
            cmd.GetTemporaryRT(tmpId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tmpId2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            // Create render target identifier
            tmpRT1 = new RenderTargetIdentifier(tmpId1);
            tmpRT2 = new RenderTargetIdentifier(tmpId2);

            // Activate render target
            ConfigureTarget(tmpRT1);
            ConfigureTarget(tmpRT2);

        }

        // Execute Render Pass ////////////////////////////////////////////////////////////////////////////
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

            // enabled?
            if (!isEnabled) return;

            // Get camera color texture
            cameraColorTexture = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get("BrightnessPP");

            cmd.SetGlobalTexture("_MainTex", cameraColorTexture);

            // Post-processing
            if (ppSettings.copyToFrameBuffer) {
                cmd.Blit(cameraColorTexture, tmpRT1, mat);
                cmd.Blit(tmpRT1, cameraColorTexture);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Cleanup of Render Pass; Release Render Target /////////////////////////////////////////////////
        public override void FrameCleanup(CommandBuffer cmd) {
        }

    }
}