using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;

public class DrawObjectsRendererFeature : ScriptableRendererFeature {
    class DrawObjectsRenderPass : ScriptableRenderPass {

        new ProfilingSampler profilingSampler;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            
            SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
            ShaderTagId shaderTagId = new ShaderTagId("VFX");

            DrawingSettings drawingSettings = CreateDrawingSettings(
                shaderTagId,
                ref renderingData,
                sortingCriteria
            );
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
            RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;

            var cmd = CommandBufferPool.Get("DrawObjectsRenderPass");
            profilingSampler = new ProfilingSampler("DrawObjectsRenderPass");
            using (new ProfilingScope(cmd, profilingSampler)) {

                // flush out cmd before rendering
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
                
            }

        }

    }

    DrawObjectsRenderPass drawObjectsRenderPass;

    /// <inheritdoc/>
    public override void Create()
    {
        drawObjectsRenderPass = new DrawObjectsRenderPass();

        // Configures where the render pass should be injected.
        drawObjectsRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(drawObjectsRenderPass);
    }
}


