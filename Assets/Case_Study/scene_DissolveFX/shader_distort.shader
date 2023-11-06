Shader "PostProcessing/Distort"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "" {}
        _NoiseIntensity ("Noise Intensity", Range(0, 50)) = 1
        _FlowSpeed ("Flow Speed", Vector) = (1,1,1,1)

        [Space(10)]
        _TestFac ("TestFac", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Overlay" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Overlay"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

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
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _NoiseTex_ST;
            float4 _NoiseTex_TexelSize;
            CBUFFER_END

            float _NoiseIntensity;
            float2 _FlowSpeed;
            float _TexScale;

            sampler2D _NoiseTex;
            sampler2D _CameraTransparentTexture;
            float4 _CameraTransparentTexture_TexelSize;

            float4 _TestFac;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float noise = tex2D(_NoiseTex, i.uv * _TexScale + _Time.x * _FlowSpeed).r * 2 - 1;
                float2 uv = i.uv + noise * _CameraTransparentTexture_TexelSize.xy * _NoiseIntensity;

                half3 col = tex2D(_CameraTransparentTexture, uv).rgb;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
