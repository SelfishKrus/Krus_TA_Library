using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RaindropRenderPassFeature : ScriptableRendererFeature
{
    // Settings类, 用于在RenderFeature面板上传参 /////////////////////
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;    // 设置pass的渲染顺序
        public Shader shader;   // 后处理时执行的shader
    }

    // 实例化 ////////////////////////////////////////
    public Settings settings = new Settings();
    RaindropRenderPass raindropRenderPass;

    // Create(), 进行初始化操作，可以把settings里的参数从面板上赋予给RaindropRenderPass ////////////////////
    public override void Create()
    {
        this.name = "RaindropRenderPass";    // 外部显示名字
        raindropRenderPass = new RaindropRenderPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);  // 初始化Pass
    }

    // AddRenderPasses(), 将raindropRenderPass加入队列; 把相机输出给到raindropRenderPass////////////////////
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {   
        // 把相机输出给到raindropRenderPass
        raindropRenderPass.Setup(renderer.cameraColorTarget);
        // 将raindropRenderPass加入队列
        renderer.EnqueuePass(raindropRenderPass);
    }
}

// ScriptableRenderPass类，核心渲染逻辑 ////////////////////
public class RaindropRenderPass : ScriptableRenderPass
{
    static readonly string k_RenderTag = "ColorTint Effects";    // 设置渲染 Tags
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");   // 设置主贴图
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");    // 设置储存图像信息

    RaindropVolumeComponent raindrop;           // 传递到volume
    Material raindropMat;     // 后处理使用材质
    RenderTargetIdentifier currentTarget;   // 设置当前渲染目标

    // 构造器
    #region  设置渲染事件
    public RaindropRenderPass(RenderPassEvent evt, Shader shader_local)   // 输入渲染顺序位置 & Shader
    {
        renderPassEvent = evt;         // 设置渲染事件的位置
        var shader = shader_local;  // 输入Shader信息
        // 判断如果不存在Shader
        if (shader = null)         // Shader如果为空提示
        {
            Debug.LogError("No specifed shader");
            return;
        }
        // 如果存在新建材质
        raindropMat = CoreUtils.CreateEngineMaterial(shader_local);
    }
    #endregion

    // 初始化函数
    #region 初始化
    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }
    #endregion

    // 渲染函数
    #region 渲染
    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;      // 获取摄像机属性
        var camera = cameraData.camera;                         // 传入摄像机
        var source = currentTarget;                             // 获取渲染图片
        int destination = TempTargetId;                         // 渲染结果图片

        raindropMat.SetFloat("_Size", raindrop.size.value);   // 获取volume component的颜色
        cmd.SetGlobalTexture(MainTexId, source);                // 获取当前摄像机渲染的图片
        cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);                          // 设置后处理
        cmd.Blit(destination, source, raindropMat, 0);    // 传入颜色校正
    }
    #endregion

    // 执行函数
    #region 执行
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // 判断材质是否为空
        if (raindropMat == null)
        {
            Debug.LogError("Material Initialization failed");
            return;
        }

        // 判断是否开启后处理
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }
        
        // 渲染设置
        var stack = VolumeManager.instance.stack;          // 传入volume
        raindrop = stack.GetComponent<RaindropVolumeComponent>();       // 拿到我们的volume
        if (raindrop == null)
        {
            Debug.LogError(" Unable to get Volume Component ");
            return;
        }

        var cmd = CommandBufferPool.Get(k_RenderTag);   // 设置渲染标签
        Render(cmd, ref renderingData);                 // 设置渲染函数
        context.ExecuteCommandBuffer(cmd);              // 执行函数
        CommandBufferPool.Release(cmd);                 // 释放
    }
    #endregion


}


