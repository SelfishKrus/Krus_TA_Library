using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace SJSJ.BattleRendererApplication.RenderPipeline {

    [Serializable]
    public class RadialBlurPPSettings {

        // Render Pass Settings
        [Header("Render Pass Settings")]
        public bool isEnabled;
        public bool copyToFrameBuffer = true;

        [HideInInspector]
        public string targetName = "_MainTex";  // The texture camera'll render to & shader'll sample from

        public Material mat;

        // Shader Settings
        [Space(10)]
        [Header("Shader Settings")]

        public Vector2 _BlurCenter = new Vector2(0.5f, 0.5f);

        [Range(-1f, 1f)] public float _BlurStep = 0.14f;
        [Range(0.01f, 0.5f)] public float _MaskInnerRadius = 0.01f;
        [Range(0.01f, 0.499f)] public float _MaskOuterRadius = 0.07f;

    }

}