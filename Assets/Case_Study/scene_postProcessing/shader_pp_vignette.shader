Shader "PostProcessing/Vignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "" {}
        // _CameraImage ("Camera Image", 2D) = "" {}
        _FallOff("FallOff", Range(0, 5)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Overlay" "IgnoreProjector"="True"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _FallOff;
            sampler2D _CameraImage;
            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half2 coord = (i.uv - 0.5);
                half dst = length(coord) * _FallOff;
                half dst_2 = dst * dst + 1.0;
                half vignette = 1 / (dst_2 * dst_2);
                
                half3 col = tex2D(_MainTex, i.uv).rgb;
                col *= vignette;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
