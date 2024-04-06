using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class AsciiRendererFeature : ScriptableRendererFeature
{

    //////////////
    // Settings // 
    //////////////

    [System.Serializable]
    public class AsciiSettings
    {
        [Header("Render Pass")]
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public string colorTargetDestinationID = "_CamColTex";

    }

    //////////////////////
    // Renderer Feature // 
    //////////////////////

    public AsciiSettings settings = new AsciiSettings();

    Material m_Material;

    AsciiRenderPass m_RenderPass = null;



    public override void Create()
    {
        m_RenderPass = new AsciiRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_RenderPass);
        m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Normal);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {

    }

    protected override void Dispose(bool disposing)
    {
        m_RenderPass.Dispose();
    }

    //////////////////////
    //   Renderer Pass  // 
    //////////////////////

    internal class AsciiRenderPass : ScriptableRenderPass
    {
        ProfilingSampler m_profilingSampler = new ProfilingSampler("Ascii");
        Material m_material;
        RTHandle m_cameraColorTarget;
        RTHandle rtCustomColor, rtTempColor;
        AsciiSettings m_settings;

        public AsciiRenderPass(AsciiSettings settings)
        {
            m_material = settings.material;
            this.m_settings = settings;
            renderPassEvent = m_settings.renderPassEvent;
        }

        public void SetTarget(RTHandle colorHandle)
        {
            m_cameraColorTarget = colorHandle;
        }

        // Pass Data //

        public void PassShaderData(Material material)
        {

        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.colorFormat = RenderTextureFormat.ARGB32;
            colorDesc.depthBufferBits = 0;

            var renderer = renderingData.cameraData.renderer;
            // set target
            m_cameraColorTarget = renderer.cameraColorTargetHandle;

            // Set up temporary color buffer (for blit)

            RenderingUtils.ReAllocateIfNeeded(ref rtCustomColor, colorDesc, name: "_RTCustomColor");
            RenderingUtils.ReAllocateIfNeeded(ref rtTempColor, colorDesc, name: "_RTTempColor");

            ConfigureTarget(m_cameraColorTarget);
            ConfigureTarget(rtCustomColor);
            ConfigureTarget(rtTempColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;

            if (m_material == null)
                return;

            if (m_cameraColorTarget.rt == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (UnityEngine.Rendering.ProfilingScope profilingScope = new UnityEngine.Rendering.ProfilingScope(cmd, m_profilingSampler))
            {
                // PassShaderData(m_material);
                m_material.SetTexture(m_settings.colorTargetDestinationID, m_cameraColorTarget);

                RTHandle rtCamera = renderingData.cameraData.renderer.cameraColorTargetHandle;

                Blitter.BlitCameraTexture(cmd, m_cameraColorTarget, rtCustomColor, m_material, 0);
                Blitter.BlitCameraTexture(cmd, rtCustomColor, m_cameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);

        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            // rtCustomColor.Release();
            // rtTempColor.Release();
        }

        public void Dispose()
        {
            RTHandles.Release(rtCustomColor);
            RTHandles.Release(rtTempColor);
        }
    }
}