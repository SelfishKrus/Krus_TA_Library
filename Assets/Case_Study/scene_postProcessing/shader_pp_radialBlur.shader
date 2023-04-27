Shader "PostProcessing/RadialBlur"
{
    Properties
    {   
        [HideInInspector] _MainTex("Texture", 2D) = "" {}
        
        [Header(Main)]
        _BlurCenter ("Blur Center", Vector) = (0.5, 0.5, 0, 0)
        _BlurStep ("Blur Step", Range(-1, 1)) = 0.14
        [HideInInspector] _BlurRadius ("Blur Radius", Range(0, 10)) = 3
        [HideInInspector] [IntRange] _Iteration ("Iteration", Range(1, 16)) = 8

        [Space(50)]

        [Header(Mask)]
        _MaskInnerRadius ("Mask Inner Radius", Range(0.01, 0.5)) = 0.01
        _MaskOuterRadius ("Mask Outer Radius", Range(0.01, 0.499)) = 0.07

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
            float4 _MainTex_ST;
            
            half2 _BlurCenter;
            half _BlurStep;
            half _BlurRadius;

            half _MaskInnerRadius;
            half _MaskOuterRadius;

            int _Iteration;
            
            

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // radial blur
                half2 blurDir = _BlurCenter - i.uv;
                half dist = length(blurDir);

                half3 accumCol = 0;
                _BlurStep *= 0.05;

                for (int j = 0; j < _Iteration; j++)
                {   
                    accumCol += tex2D(_MainTex, i.uv).rgb;
                    i.uv += blurDir * _BlurStep;
                }
                half3 blurCol = accumCol / _Iteration;
                half3 originalCol = tex2D(_MainTex, i.uv).rgb;

                // mask 
                half mask = saturate(smoothstep(_MaskInnerRadius, _MaskOuterRadius, dist));
                
                
                half3 col = lerp(originalCol, blurCol, mask * saturate(dist * _BlurRadius));
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
