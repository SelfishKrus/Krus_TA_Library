using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SJSJ.BattleRendererApplication.RenderPipeline {

    public class RadialDistortionPPRenderPass : ScriptableRenderPass {

        PostProcessingRendererFeatureSetting RFsettings;

        RadialDistortionPPSettings PPsettings;

        Material mat;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;

        int tmpId1;
        int tmpId2;

        RenderTargetIdentifier cameraColorTexture;

        public RadialDistortionPPRenderPass(PostProcessingRendererFeatureSetting RFsettings, RadialDistortionPPSettings PPsettings) {
            this.PPsettings = PPsettings;
            this.renderPassEvent = RFsettings.renderPassEvent;
            this.RFsettings = RFsettings;
        }

        // FUNCTION to set values during runtime //////////////////////////////////////////////////////////
        public void SetValues(
            Vector2 scaleUV, 
            float speed, 
            float distortionRange, 
            float distortionIntensity
            ) {
            mat.SetVector("_ScaleUV", scaleUV);
            mat.SetFloat("_Speed", speed);
            mat.SetFloat("_DistortionRange", distortionRange);
            mat.SetFloat("_DistrotionIntensity", distortionIntensity);
        }

        // To Set Render Target //////////////////////////////////////////////////////////////////////////
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            
            // Create a new material instance
            mat = new Material(PPsettings.mat);
            if (mat == null){
                Debug.LogError("RadialDistortionPPRenderPass: Material is null");
                return;
            }
            // pass values to mat
            mat.SetVector("_ScaleUV", PPsettings._ScaleUV);
            mat.SetFloat("_Speed", PPsettings._Speed);
            mat.SetFloat("_DistortionRange", PPsettings._DistortionRange);
            mat.SetFloat("_DistrotionIntensity", PPsettings._DistrotionIntensity);
            

            int downsample = RFsettings.downsample;
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
            if (!PPsettings.isEnabled) return;

            // Get camera color texture
            cameraColorTexture = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get("RadialDistortionPP");

            cmd.SetGlobalTexture("_MainTex", cameraColorTexture);

            // Post-processing
            if (PPsettings.copyToFrameBuffer) {
                cmd.Blit(cameraColorTexture, tmpRT1, mat);
                cmd.Blit(tmpRT1, cameraColorTexture);
            }


            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Cleanup of Render Pass; Release Render Target /////////////////////////////////////////////////
        public override void FrameCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(tmpId1);
            cmd.ReleaseTemporaryRT(tmpId2);
        }

    }
}