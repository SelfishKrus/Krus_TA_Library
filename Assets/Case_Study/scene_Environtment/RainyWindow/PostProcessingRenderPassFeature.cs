using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingRenderPassFeature : ScriptableRendererFeature
{
    // RenderFeature UI Settings ////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class RenderFeatureSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents; // queue
        public Material mat = null;     //post-processing material

        [Range(1,4)]
        public int downsample = 1;    // downsample factor
        public bool copyToFramebuffer;  // copy to framebuffer or not
        public string targetName = "_MainTex";  // The texture camera'll render to & shader'll sample from
    }

    public RenderFeatureSettings settings = new RenderFeatureSettings();

    // Render Pass ///////////////////////////////////////////////////////////////////////////////////////
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material mat;
        
        public int downsample;
        public bool copyToFramebuffer;
        public string targetName;        
        string profilerTag;     // tag for debug

        int tmpId1;
        int tmpId2;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;

        RenderTargetIdentifier cameraColorTexture;

        public CustomRenderPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        // Initialization of Render Pass; Set Render Target //////////////////////////////////////////////
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
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
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Get camera color texture
            cameraColorTexture = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            // Post-processing
            cmd.Blit(cameraColorTexture, tmpRT1, mat);
            if (copyToFramebuffer) {
                cmd.Blit(tmpRT1, cameraColorTexture);
            } else {
                cmd.Blit(tmpRT1, tmpRT2);
                cmd.SetGlobalTexture(targetName, tmpRT2);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Cleanup of Render Pass; Release Render Target /////////////////////////////////////////////////
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass scriptablePass;

    // Initialization of Render Feature; Pass UI Settings to Render Pass /////////////////////////////////
    public override void Create()
    {
        scriptablePass = new CustomRenderPass("CustomePostProcessing");
        scriptablePass.mat = settings.mat;
        scriptablePass.downsample = settings.downsample;
        scriptablePass.copyToFramebuffer = settings.copyToFramebuffer;
        scriptablePass.targetName = settings.targetName;

        scriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    // Set render pass order /////////////////////////////////////////////////////////////////////////////
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(scriptablePass);
    }
}