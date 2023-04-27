Shader "VFX/Distortion"
{
    Properties
    {   
        [HideInInspector] _MainTex ("Texture", 2D) = "" {}
        [HideInInspector] _CameraColorTexture ("Camera Color Texture", 2D) = "" {}

        [Header(Noise)]
        _NoiseMap ("Noise Map", 2D) = "white" {}
        _SpeedU ("Speed U", Float) = 0.5
        _SpeedV ("Speed V", Float) = 0.5
        _DistortionIntensity ("Distortion Intensity", Range(0, 1)) = 1.0
        [Space(30)]

        [Header(Mask)]
        _MaskMap ("Mask", 2D) = "white" {}

        [Space(30)]
        _Test ("Test Factor", Vector) = (0,0,0,0)
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" }
        LOD 100
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {   
            Name "VFX"
            Tags {"LightMode" = "VFX"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                half4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _CameraColorTexture;

            sampler2D _NoiseMap;
            float4 _NoiseMap_ST;

            half _SpeedU;
            half _SpeedV;
            half _DistortionIntensity;

            sampler2D _MaskMap;

            half4 _Test;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseMap);
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                // mask 
                half mask = tex2D(_MaskMap, i.uv).r ;

                // uv offset
                half t = _Time.y;
                half2 uv = i.uv;
                uv.x += t * _SpeedU;
                uv.y += t * _SpeedV;

                // noise
                half2 offset = tex2D(_NoiseMap, uv).rg;

                half2 screenUV = i.pos.xy / _ScreenParams.xy;
                half2 flowUV = screenUV + offset * _DistortionIntensity * i.color.a * mask;

                half3 camCol = tex2D(_CameraColorTexture, flowUV).rgb;
                
                half3 col = camCol;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
