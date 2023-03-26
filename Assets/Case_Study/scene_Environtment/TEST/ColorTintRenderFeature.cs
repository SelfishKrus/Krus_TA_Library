using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorTintRenderFeature: ScriptableRendererFeature
{
    [System.Serializable]

    public class Settings      // 初始设置
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;        // 设置渲染顺序  在后处理前
        public Shader shader;      // 设置后处理Shader
    }

    public Settings settings = new Settings();            // 开放设置

    ColorTintPass colorTintPass;    // 设置渲染Pass

    public override void Create() // 初始化 属性
    {
        this.name = "ColorTintPass";        // 外部显示名字
        colorTintPass = new ColorTintPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);      // 初始化Pass
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) // Pass执行逻辑
    {
        colorTintPass.Setup(renderer.cameraColorTarget);                 // 初始化Pass里的属性
        renderer.EnqueuePass(colorTintPass);
    }
}


// 定义执行Pass
public class ColorTintPass : ScriptableRenderPass
{
    static readonly string k_RenderTag = "ColorTint Effects";          // 设置渲染 Tags
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");   // 设置主贴图
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");    // 设置储存图像信息

    ColorTint colorTint;           // 传递到volume
    Material colorTintMaterial;     // 后处理使用材质

    RenderTargetIdentifier currentTarget;   // 设置当前渲染目标

    #region  设置渲染事件
    public ColorTintPass(RenderPassEvent evt, Shader ColorTintShader)        // 输入渲染位置    Shader
    {
        renderPassEvent = evt;         // 设置渲染事件的位置
        var shader = ColorTintShader;  // 输入Shader信息
        // 判断如果不存在Shader
        if (shader = null)         // Shader如果为空提示
        {
            Debug.LogError("没有指定Shader");
            return;
        }
        //如果存在新建材质
        colorTintMaterial = CoreUtils.CreateEngineMaterial(ColorTintShader);
    }
    #endregion
    // 初始化函数
    #region 初始化
    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }
    #endregion

    #region 执行
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // 判断材质是否为空
        if (colorTintMaterial == null)
        {
            Debug.LogError("材质初始化失败");
            return;
        }
        // 判断是否开启后处理
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }
        // 渲染设置
        var stack = VolumeManager.instance.stack;          // 传入volume
        colorTint = stack.GetComponent<ColorTint>();       // 拿到我们的volume
        if (colorTint == null)
        {
            Debug.LogError(" Volume组件获取失败 ");
            return;
        }

        var cmd = CommandBufferPool.Get(k_RenderTag);   // 设置渲染标签
        Render(cmd, ref renderingData);                 // 设置渲染函数
        context.ExecuteCommandBuffer(cmd);              // 执行函数
        CommandBufferPool.Release(cmd);                 // 释放
    }
    #endregion

    #region 渲染
    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;      // 获取摄像机属性
        var camera = cameraData.camera;                         // 传入摄像机
        var source = currentTarget;                             // 获取渲染图片
        int destination = TempTargetId;                         // 渲染结果图片

        colorTintMaterial.SetColor("_ColorTint", colorTint.ColorChange.value);   // 获取value 组件的颜色
        cmd.SetGlobalTexture(MainTexId, source);                // 获取当前摄像机渲染的图片
        cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);                          // 设置后处理
        cmd.Blit(destination, source, colorTintMaterial, 0);    // 传入颜色校正
    }

    #endregion
} 