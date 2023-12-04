Shader "KrusShader/FakeTyndall"
{
    Properties
    {
        _BeamCol ("Beam Color", Color) = (1,1,1,1)
        _DirtCol ("Dirt Color", Color) = (1,1,1,1)
        _DirtSpeed ("Dirt Flow Speed", Vector) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "" {}
        _Beam_ST ("Beam ST", Vector) = (1,1,0,0)
        _Noise_ST ("Noise ST", Vector) = (1,1,0,0)
        _Dirt_ST ("Dirt ST", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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
                float4 uvs0 : TEXCOORD0;
                float4 uvs1 : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 posOS : TEXCOORD3;
                float4 pos : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Beam_ST;
            float4 _Noise_ST;
            float4 _Dirt_ST;

            half3 _BeamCol;
            half3 _DirtCol;
            float2 _DirtSpeed;
            CBUFFER_END

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uvs0 = float4(v.uv * _MainTex_ST.xy + _MainTex_ST.zw, v.uv * _Beam_ST.xy + _Beam_ST.zw);
                o.uvs1 = float4(v.uv * _Noise_ST.xy + _Noise_ST.zw, v.uv * _Dirt_ST.xy + _Dirt_ST.zw);
                o.uv = v.uv;
                o.posOS = v.posOS;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv_o = i.uvs0.xy;
                float2 uv_beam = i.uvs0.zw * _Beam_ST.xy + _Beam_ST.zw;
                float2 uv_noise = i.posOS.xz * _Noise_ST.xy + _Noise_ST.zw + _Time.x * _DirtSpeed;
                float2 uv_dirt = i.posOS.xz * _Dirt_ST.xy + _Dirt_ST.zw + _Time.x * _DirtSpeed;

                half gNoise = tex2D(_MainTex, uv_noise).g;
                half gBeam = tex2D(_MainTex, uv_beam ).r;
                half gDirt = tex2D(_MainTex, uv_dirt).b;

                half3 col = _BeamCol * gNoise + _DirtCol * gDirt;
                return half4(col, gBeam);
            }
            ENDHLSL
        }
    }
}
