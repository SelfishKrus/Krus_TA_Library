using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("My Post-processing/Rainddrop")]
    public sealed class RaindropVolumeComponent : VolumeComponent, IPostProcessComponent
    {
		//鼠标放到参数上时显示的描述信息
        [Tooltip("meow")]
        //只有用官方封装的类型定义的变量才能在面板中显示
        public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);
        public ClampedFloatParameter size = new ClampedFloatParameter(0.7f, 0f, 10f);
        public ColorParameter tint = new ColorParameter(Color.white, false, false, true);
        public BoolParameter highQualityFiltering = new BoolParameter(false);
        public TextureParameter dirtTexture = new TextureParameter(null);
        public bool IsActive() => true;
        public bool IsTileCompatible() => false;
    }
}