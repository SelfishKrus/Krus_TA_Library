Shader "Krus/TriplanarProjection"
{
    Properties
    {
        _Map ("Texture", 2D) = "white" {}
        _Map_ST ("Map ST", Vector) = (1,0,0,0)
        _BlendOffset ("BLend Offset", Range(0, 1)) = 0.01

        _Test ("Test", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "K_TriPlanarProjection.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 pos : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            float4 _Test;
            Texture2D _Map;     SamplerState sampler_Map;
            float2 _Map_ST;
            float _BlendOffset;

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.pos = TransformObjectToHClip(IN.posOS.xyz);
                OUT.posWS = TransformObjectToWorld(IN.posOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (v2f IN) : SV_Target
            {
                half3 col = TriplanarSampling(_Map, sampler_Map, IN.normalWS, IN.posWS, _Map_ST,_BlendOffset);
                
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
