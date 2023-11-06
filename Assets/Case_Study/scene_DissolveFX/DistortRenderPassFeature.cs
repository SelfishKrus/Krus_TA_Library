using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistortRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class DistortSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material distortMaterial;
    }
    public DistortSettings distortSettings = new DistortSettings();

    class DistortRenderPass : ScriptableRenderPass
    {
        // 成员
        public Material distortMaterial;
        public Texture noiseTexture;

        RenderTargetIdentifier cameraColorTexture;

        int tmpId1;
        int tmpId2;

        RenderTargetIdentifier tmpRT1;
        RenderTargetIdentifier tmpRT2;

        public DistortVolumeComponent distortVolumeComponent;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            tmpId1 = Shader.PropertyToID("_Temp1");
            tmpId2 = Shader.PropertyToID("_Temp2");

            cmd.GetTemporaryRT(tmpId1, cameraTextureDescriptor);
            cmd.GetTemporaryRT(tmpId2, cameraTextureDescriptor);

            tmpRT1 = new RenderTargetIdentifier(tmpId1);
            tmpRT2 = new RenderTargetIdentifier(tmpId2);

            ConfigureTarget(tmpRT1);
            ConfigureTarget(tmpRT2);


        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraColorTexture = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get("DistortRenderPass");

            var stack = VolumeManager.instance.stack;
            distortVolumeComponent = stack.GetComponent<DistortVolumeComponent>();
            if (distortVolumeComponent == null) return;
            if (distortMaterial != null)
            {
                distortMaterial.SetFloat("_NoiseIntensity", distortVolumeComponent.noiseIntensity.value);
                distortMaterial.SetVector("_FlowSpeed", distortVolumeComponent.flowSpeed.value);
                distortMaterial.SetFloat("_TexScale", distortVolumeComponent.texScale.value);
            }

            cmd.Blit(cameraColorTexture, tmpRT1, distortMaterial);
            cmd.Blit(tmpRT1, cameraColorTexture);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    DistortRenderPass m_ScriptablePass;
    DistortVolumeComponent distortVolumeComponent;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new DistortRenderPass();

        m_ScriptablePass.distortMaterial = distortSettings.distortMaterial;
        m_ScriptablePass.renderPassEvent = distortSettings.renderPassEvent;
    }

    /// <inheritdoc/>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


