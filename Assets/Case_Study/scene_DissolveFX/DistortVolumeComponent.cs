// Template 
using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Krus Post-processing/Distort")]
    public sealed class DistortVolumeComponent : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("meow")]
        public ClampedFloatParameter noiseIntensity = new ClampedFloatParameter(5f, 0f, 50f);
        public Vector2Parameter flowSpeed = new Vector2Parameter(new Vector2(0.5f, 0.5f));
        public ClampedFloatParameter texScale = new ClampedFloatParameter(0.1f, 0f, 3f);

        public bool IsActive() => noiseIntensity.value > 0f;
        public bool IsTileCompatible() => false;
    }
}