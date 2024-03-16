using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
using SerializableAttribute = System.SerializableAttribute;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

internal class StreakBloomRendererFeature : ScriptableRendererFeature
{   

    //////////////
    // Settings // 
    //////////////

    [System.Serializable]
    public class StreakBloomSettings 
    {   
        [Header("Render Pass")]
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public string colorTargetDestinationID = "_CamColTex";
    }

    //////////////////////
    // Renderer Feature // 
    //////////////////////

    public StreakBloomSettings settings = new StreakBloomSettings();

    Material m_Material;

    StreakBloomRenderPass m_RenderPass = null;

    public override void Create()
    {
        m_RenderPass = new StreakBloomRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        // if (renderingData.cameraData.camera.cameraType != CameraType.Game && renderingData.cameraData.camera.cameraType != CameraType.SceneView)
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
    }

    //////////////////////
    //   Renderer Pass  // 
    //////////////////////

    internal class StreakBloomRenderPass : ScriptableRenderPass
    {   
        ProfilingSampler m_profilingSampler = new ProfilingSampler("StreakBloom");
        Material m_material;
        RenderTargetIdentifier m_cameraColorTarget;
        int camId;
        RenderTargetIdentifier tempRT0, tempRT1;
        int tempId0, tempId1;
        StreakBloomSettings m_settings;

        const int MaxMipMapLevel = 16;
        (RTHandle down, RTHandle up)[] mips = new (RTHandle down, RTHandle up) [MaxMipMapLevel];

        static class ShaderIDs
        {
            internal static readonly int InputTex = Shader.PropertyToID("_InputTex");
            internal static readonly int HighTex = Shader.PropertyToID("_HighTex");
            internal static readonly int CamColTex = Shader.PropertyToID("_CamColTex");
            internal static readonly int rtTempColor0 = Shader.PropertyToID("_rtTempColor0");
            internal static readonly int rtTempColor1 = Shader.PropertyToID("_rtTempColor1");
        }

        public StreakBloomRenderPass(StreakBloomSettings settings)
        {   
            this.m_settings = settings;
            renderPassEvent = m_settings.renderPassEvent;
            m_material = m_settings.material;
        }

        public void SetTarget(RTHandle colorHandle)
        {
            m_cameraColorTarget = colorHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {   
            Dispose(cmd);

            var camDesc = renderingData.cameraData.cameraTargetDescriptor;
            camDesc.colorFormat = RenderTextureFormat.ARGB32;
            camDesc.depthBufferBits = 0;

            var width = camDesc.width / 2;
            var height = camDesc.height / 4;

            // Create temporary render target
            tempId0 = Shader.PropertyToID("tempRT0");
            tempId1 = Shader.PropertyToID("temp1");
            camId = Shader.PropertyToID("camRT");
            cmd.GetTemporaryRT(tempId0, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tempId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(camId, camDesc.width, camDesc.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            // Create render target identifier
            tempRT0 = new RenderTargetIdentifier(tempId0);
            tempRT1 = new RenderTargetIdentifier(tempId1);
            m_cameraColorTarget = new RenderTargetIdentifier(camId);
            
            // Activate render target
            ConfigureTarget(tempRT0);    
            ConfigureTarget(tempRT1);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_material == null)
                return;

            // return if color target is invalid
            // if (m_cameraColorTarget == null)
            //     return;
            
            CommandBuffer cmd = CommandBufferPool.Get();

            using (UnityEngine.Rendering.ProfilingScope profilingScope = new UnityEngine.Rendering.ProfilingScope(cmd, m_profilingSampler))
            {   

                cmd.SetGlobalTexture(ShaderIDs.CamColTex, m_cameraColorTarget);
                // pyramid = GetPyramid(renderingData.cameraData.camera);

                // Prefilter
                // CameraColorTarget -> Prefilter -> MIP 0
                cmd.SetRenderTarget(tempRT0);
                cmd.Blit(m_cameraColorTarget, tempRT0, m_material, 0);

                cmd.SetRenderTarget(m_cameraColorTarget);
                cmd.Blit(tempRT0, m_cameraColorTarget);

            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);

        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose(CommandBuffer cmd)
        {   
            cmd.ReleaseTemporaryRT(tempId0);
            cmd.ReleaseTemporaryRT(tempId1);

            // for (int i = 0; i < MaxMipMapLevel; i++)
            // {
            //     if (mips[i].down != null) RTHandles.Release(mips[i].down);
            //     if (mips[i].up != null) RTHandles.Release(mips[i].up);
            // }
        }
    }

}