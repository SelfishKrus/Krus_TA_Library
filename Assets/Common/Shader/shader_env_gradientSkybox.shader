Shader "Environment/gradientSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HorizonPos ("Horizon Position", Range(-1, 1)) = 0.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.0

        _TopCol ("Top Color", Color) = (0,0,0,0)
        _BottomCol ("Bottom Color", Color) = (0,0,0,0)

        _TestParam ("Test Param", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Background"  "Queue"="Background" "PreviewType"="Sphere"}
        LOD 100
        ZWrite Off
        Cull Front
        
        // write a function that gradients 3 colors


        Pass
        {
            Tags {}

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
                float3 posOS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            float4 _TestParam;
            float _HorizonPos;
            float _Smoothness;

            float4 _TopCol;
            float4 _BottomCol;

            sampler2D _MainTex;

            float2 UVToLongLat (float2 uv)
            {
                float2 ll;
                ll.x = uv.x * 2 * 3.1415926535897932384626433832795;
                ll.y = uv.y * 3.1415926535897932384626433832795;
                return ll;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.posOS = v.posOS.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                float2 uv = UVToLongLat(i.uv);
                half3 noise = tex2D(_MainTex, uv).rgb;

                half3 col;
                float fac = smoothstep(_HorizonPos - _Smoothness, _HorizonPos + _Smoothness, i.posOS.y);
                col = lerp(_BottomCol.rgb, _TopCol.rgb, fac);
                col *= noise;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
