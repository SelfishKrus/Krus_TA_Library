Shader "KrusShader/OceanSphere"
{
    Properties
    {
        _Cubemap ("Cubemap", 2D) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}
        LOD 100
        ZWrite On
        Cull Back

        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 posWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _Cubemap_ST;
            CBUFFER_END

            TEXTURECUBE(_Cubemap);    SAMPLER(sampler_Cubemap);

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.posWS = TransformObjectToWorld(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _Cubemap);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 vec = normalize(i.posWS.xyz);
                half3 col = SAMPLE_TEXTURECUBE(_Cubemap, sampler_Cubemap, vec).rgb;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
