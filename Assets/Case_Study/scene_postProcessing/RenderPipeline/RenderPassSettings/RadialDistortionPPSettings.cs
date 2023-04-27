using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace SJSJ.BattleRendererApplication.RenderPipeline {

    [Serializable]
    public class RadialDistortionPPSettings {

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

        // always larger than 0
        public Vector2 _ScaleUV = new Vector2(1, 1);
        public float _Speed = 1;

        [Range(0, 1)]
        public float _DistortionRange = 0.6f;
        public float _DistrotionIntensity = 3f;

    }

}