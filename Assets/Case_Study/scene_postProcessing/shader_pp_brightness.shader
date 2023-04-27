Shader "PostProcessing/Brightness"
{
    Properties
    {   
        [HideInInspector] _MainTex("Texture", 2D) = "" {}
        _Brightness ("Brightness", Range(0, 1)) = 0.5

        _Test("Test", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Overlay"}
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
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            half _Brightness;
            float4 _Test;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                // get render texture of current camera
                // half2 coord = (i.uv - 0.5);
                // half dst = length(coord) * _FallOff;
                // half dst_2 = dst * dst + 1.0;
                // half vignette = 1 / (dst_2 * dst_2);
                
                half3 col = tex2D(_MainTex, i.uv).rgb * _Brightness;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
