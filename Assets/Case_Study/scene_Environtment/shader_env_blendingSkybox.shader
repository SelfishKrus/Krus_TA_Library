Shader "Environment/BlendingSkybox"
{
    Properties
    {
        [Header(HDRI)]
        [HDR] _MainTex ("Texture", Cube) = "red" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0, 10)) = 1
        _Contrast ("Contrast", Range(0, 1)) = 1
        _Saturation ("Saturation", Range(0, 1)) = 1

        [Header(Customized Sky Settings)]
        _HorizonCol ("Horizon Color", Color) = (0.5,0.5,0.5,1)
        _ZenithCol ("Zenith Color", Color) = (0.5,1,0.5,1)

        _MaskRange ("Mask Range", Range(0, 1)) = 0.5

    }
    SubShader
    {
        Tags { "RenderType"="Background" "Queue"="Background" "PreviewType"="Skybox"}
        LOD 100
        ZWrite Off
        Cull Off

        Pass
        {
            
            Tags {}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };


            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_HDR;
            float _Brightness;
            float _Saturation;
            float _Contrast;
            float4 _Tint;

            float4 _HorizonCol;
            float4 _ZenithCol;

            float _MaskRange;
            CBUFFER_END

            TEXTURECUBE(_MainTex);    SAMPLER(sampler_MainTex);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.normalWS = TransformObjectToWorldDir(v.normal);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                // preparation 
                half3 dir = normalize(i.posWS);
                half3 up = half3(0, 1, 0);
                half cosTheta = dot(dir, up);

                _MaskRange *= 10;

                // HDRI //////////////////////////////////////////
                half3 colHDR = DecodeHDREnvironment(SAMPLE_TEXTURECUBE(_MainTex, sampler_MainTex, dir), _MainTex_HDR);
                // post-processing 
                // brightness //
                colHDR *= _Brightness;
                // contrast //
                half3 avgCol = 0.5;
                colHDR = lerp(avgCol, colHDR, _Contrast);
                // saturation // 
                half gray = dot(colHDR, half3(0.2125, 0.7154, 0.0721));
                half3 grayCol = gray;
                colHDR = lerp(grayCol, colHDR, _Saturation);
                // tint //
                colHDR *= _Tint.rgb;

                // Procedural Sky //////////////////////////////////////////
                float3 absPosWS = abs(i.posWS);
                half3 pSkyCol = lerp(_HorizonCol.rgb, _ZenithCol.rgb, saturate(cosTheta));

                // Blend //////////////////////////////////////////
                // Mask between Procedural Sky and HDRI
                half3 mask = saturate(cosTheta);
                mask = smoothstep(0, _MaskRange, mask);
                // blend
                half3 col = lerp(pSkyCol, colHDR, mask);

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
