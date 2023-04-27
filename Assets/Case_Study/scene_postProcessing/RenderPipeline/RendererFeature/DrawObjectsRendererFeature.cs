using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RendererUtils;

namespace SJSJ.BattleRendererApplication.RenderPipeline {

    public class DrawObjectsRendererFeature : ScriptableRendererFeature {

        public class GetCameraTextureRenderPass : ScriptableRenderPass {

            RenderTargetIdentifier tmpRT1;
            RenderTargetIdentifier tmpRT2;

            int tmpId1;
            int tmpId2;

            RenderTargetIdentifier cameraColorTexture;

            string targetName;

            public GetCameraTextureRenderPass(RenderPassEvent renderPassEvent, string targetName) {
                this.renderPassEvent = renderPassEvent;
                this.targetName = targetName;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {

                var width = cameraTextureDescriptor.width;
                var height = cameraTextureDescriptor.height;

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

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

                // Get camera color texture
                cameraColorTexture = renderingData.cameraData.renderer.cameraColorTarget;
                CommandBuffer cmd = CommandBufferPool.Get("GetCameraTextureRenderPass");

                cmd.SetGlobalTexture("_CameraColorTexture", cameraColorTexture);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd) {
            }

        }


        class DrawObjectsRenderPass : ScriptableRenderPass {

            new ProfilingSampler profilingSampler;

            int tmpId1;
            int tmpId2;

            RenderTargetIdentifier tmpRT1;
            RenderTargetIdentifier tmpRT2;

            RenderTargetIdentifier cameraColorTexture;

            public DrawObjectsRenderPass(RenderPassEvent renderPassEvent) {
                this.renderPassEvent = renderPassEvent;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                
                var cmd = CommandBufferPool.Get("DrawObjectsRenderPass");

                // Draw Objects
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

                profilingSampler = new ProfilingSampler("DrawObjectsRenderPass");
                using (new ProfilingScope(cmd, profilingSampler)) {

                    // Execute cmd to make settings available
                    // flush out cmd before rendering
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
                    
                }

            }

        }

        GetCameraTextureRenderPass getCameraTextureRenderPass;
        [SerializeField]
        RenderPassEvent getCameraTextureRenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        [SerializeField]
        string targetName = "_CameraColorTexture";

        DrawObjectsRenderPass drawObjectsRenderPass;
        [SerializeField]
        RenderPassEvent drawObjectsRenderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        /// <inheritdoc/>
        public override void Create(){

            drawObjectsRenderPass = new DrawObjectsRenderPass(drawObjectsRenderPassEvent);

            getCameraTextureRenderPass = new GetCameraTextureRenderPass(getCameraTextureRenderPassEvent, targetName);

        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
            renderer.EnqueuePass(getCameraTextureRenderPass);
            renderer.EnqueuePass(drawObjectsRenderPass);
        }
    }
}

