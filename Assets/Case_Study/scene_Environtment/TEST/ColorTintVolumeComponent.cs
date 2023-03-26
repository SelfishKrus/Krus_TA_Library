using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class ColorTint : VolumeComponent
{
    // 设置颜色参数
    public ColorParameter ColorChange = new ColorParameter(Color.white, true);
}