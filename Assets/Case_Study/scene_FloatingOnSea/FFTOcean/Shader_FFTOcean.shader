Shader "KrusShader/FFTOcean"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" "IgnoreProjector"="True"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma target 5.0
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
            };

            float4 _MainTex_ST;

            float _Tile0, _Tile1, _Tile2, _Tile3;
			int _DebugTile0, _DebugTile1, _DebugTile2, _DebugTile3;
			int _DebugLayer0, _DebugLayer1, _DebugLayer2, _DebugLayer3;
			int _ContributeDisplacement0, _ContributeDisplacement1, _ContributeDisplacement2, _ContributeDisplacement3;


            TEXTURE2D_ARRAY(_DisplacementTextures);    SAMPLER(sampler_DisplacementTextures);
            TEXTURE2D_ARRAY(_SlopeTextures);    SAMPLER(sampler__SlopeTextures);
            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            v2f vert (appdata v)
            {
                v2f o;

                half3 displacement1 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, v.uv * _Tile0, 0, 0) * _DebugLayer0 * _ContributeDisplacement0;
                half3 displacement2 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, (v.uv - 0.5f) * _Tile1, 1, 0) * _DebugLayer1 * _ContributeDisplacement1;
                half3 displacement3 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, (v.uv - 1.125f) * _Tile2, 2, 0) * _DebugLayer2 * _ContributeDisplacement2;
                half3 displacement4 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, (v.uv - 1.25f) * _Tile3, 3, 0) * _DebugLayer3 * _ContributeDisplacement3;
				half3 displacement = displacement1 + displacement2 + displacement3;
                v.posOS.xyz += displacement1 * 0.001;

                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 displacement1 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, i.uv * _Tile0, 0) * _DebugLayer0 * _ContributeDisplacement0;
                half3 displacement2 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, (i.uv - 0.5f) * _Tile1, 1) * _DebugLayer1 * _ContributeDisplacement1;
                half3 displacement3 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, (i.uv - 1.125f) * _Tile2, 2) * _DebugLayer2 * _ContributeDisplacement2;
                half3 displacement4 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, (i.uv - 1.25f) * _Tile3, 3) * _DebugLayer3 * _ContributeDisplacement3;
				half3 displacement = displacement1 + displacement2 + displacement3;

                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
