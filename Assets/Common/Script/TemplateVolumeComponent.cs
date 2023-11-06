// Template 
using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("My Post-processing/Template")]
    public sealed class TemplateVolumeComponent : VolumeComponent, IPostProcessComponent
    {
        //鼠标放到参数上时显示的描述信息
        [Tooltip("meow")]
        //只有用官方封装的类型定义的变量才能在面板中显示
        public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);
        public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0f, 1f);
        public ColorParameter tint = new ColorParameter(Color.white, false, false, true);
        public BoolParameter highQualityFiltering = new BoolParameter(false);
        public TextureParameter dirtTexture = new TextureParameter(null);
        // => 类似于 return
        public bool IsActive() => intensity.value > 0f;
        public bool IsTileCompatible() => false;
    }
}