using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SJSJ.BattleRendererApplication.RenderPipeline {

    public class RadialBlurPPRenderPass : ScriptableRenderPass {

        PostProcessingRendererFeatureSetting rfSettings;

        RadialBlurPPSettings ppSettings;
        bool isEnabled;

        Material mat;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;

        int tmpId1;
        int tmpId2;

        RenderTargetIdentifier cameraColorTexture;

        public RadialBlurPPRenderPass(PostProcessingRendererFeatureSetting RFsettings, RadialBlurPPSettings PPsettings) {
            this.ppSettings = PPsettings;
            this.renderPassEvent = RFsettings.renderPassEvent;
            this.rfSettings = RFsettings;
            
            Initialize();

            this.isEnabled = ppSettings.isEnabled;
            mat.SetVector("_BlurCenter", ppSettings._BlurCenter);
            mat.SetFloat("_BlurStep", ppSettings._BlurStep);
            mat.SetFloat("_MaskInnerRadius", ppSettings._MaskInnerRadius);
            mat.SetFloat("_MaskOuterRadius", ppSettings._MaskOuterRadius);
        }

        public void Initialize() {
            mat = new Material(ppSettings.mat);
            // Pass values to shader
        }

        // FUNCTION to set values during runtime //////////////////////////////////////////////////////////
        public void SetValues(Vector2 blurCenter, float blurStep, float maskInnerRadius, float maskOuterRadius) {
            mat.SetVector("_BlurCenter", blurCenter);
            mat.SetFloat("_BlurStep", blurStep);
            mat.SetFloat("_MaskInnerRadius", maskInnerRadius);
            mat.SetFloat("_MaskOuterRadius", maskOuterRadius);
        }

        public void SetEnabled(bool value) {
            this.isEnabled = value;
        }

        public void SetStep(float step) {
            mat.SetFloat("_BlurStep", step);
        }

        // To Set Render Target //////////////////////////////////////////////////////////////////////////
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {

            if (mat == null){
                return;
            }

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
            CommandBuffer cmd = CommandBufferPool.Get("RadialBlurPP");

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