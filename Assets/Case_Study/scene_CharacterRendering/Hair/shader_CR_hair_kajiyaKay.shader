Shader "CharacterRendering/Hair_KajiyaKay"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        [NoScaleOffset] _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        [NoScaleOffset] _MetallicTex ("Metallic Map", 2D) = "black" {}
        [NoScaleOffset] _RoughnessTex ("Roughness Map", 2D) = "" {}
        _RoughnessScale ("Roughness Scale", Range(0, 1)) = 1.0
        [NoScaleOffset] _AOTex ("AO Map", 2D) = "white" {}
        _AOScale ("AO Scale", Range(0, 3)) = 1.0
        [Space(50)]

        _ShiftTangentTex ("Shift Tangent Map", 2D) = "white" {}
        _SpecCol ("Specular Color 1", Color) = (1,0,0,1)
        _Shift ("Shift", Vector) = (0.5,1,0,0)
        _Glossiness ("Glossiness", Vector) = (1, 1, 0, 0)
        _Alpha ("Alpha Intensity", Range(1, 10)) = 1
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.25
        [Space(50)]


        _TestFactor ("Test Factor", float) = 0.5
    }
    SubShader
    {   
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {   
            
            Name "PreZ"
            Tags {"LightMode"="DepthOnly"}

            Cull Off
            ZWrite On
            ZTest Less
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert_PBR
            #pragma fragment frag_preZ

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/Common/Shader/SJSJ_PBRSetup.hlsl"
            #include "Assets/Common/Shader/SJSJ_BRDF.hlsl"
            #include "hairMain.hlsl"
            ENDHLSL

        }

        Pass
        {   
            Name "DrawHair"
            Tags {"LightMode"="UniversalForward"}

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert_PBR
            #pragma fragment frag_hair

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/Common/Shader/SJSJ_PBRSetup.hlsl"
            #include "Assets/Common/Shader/SJSJ_BRDF.hlsl"
            #include "hairMain.hlsl"
            
            ENDHLSL
        }

    }
}
