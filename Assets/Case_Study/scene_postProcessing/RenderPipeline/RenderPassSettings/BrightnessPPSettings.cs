using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

    [Serializable]
    public class BrightnessPPSettings 
    {

        [Header("Render Pass Settings")]
        public bool isEnabled;
        public bool copyToFrameBuffer = true;

        [HideInInspector]
        public string targetName = "_MainTex";  // The texture camera'll render to & shader'll sample from
        
        public Material mat;
        
        [Space(10)]
        [Header("Shader Settings")]

        [Range(0,1)]
        public float _Brightness = 0.5f;
        
    }