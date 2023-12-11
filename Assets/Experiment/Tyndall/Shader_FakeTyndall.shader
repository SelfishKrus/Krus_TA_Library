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
<<<<<<< HEAD
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "DisableBatching"="True"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
=======
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
>>>>>>> origin/main
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
<<<<<<< HEAD
                float3 normalOS : NORMAL;
                float3 tangentOS : TANGENT;
=======
>>>>>>> origin/main
            };

            struct v2f
            {
                float4 uvs0 : TEXCOORD0;
                float4 uvs1 : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 posOS : TEXCOORD3;
<<<<<<< HEAD
                float3 normalWS : TEXCOORD4;
                float3 posWS : TEXCOORD5;
=======
>>>>>>> origin/main
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
<<<<<<< HEAD
                
                
                float3 centerOS = float3(0,0,0);
                float3 fixedAxisOS = float3(1,0,0);
                float3 camPosOS = TransformWorldToObject(_WorldSpaceCameraPos);
                float3 camDirOS = normalize(camPosOS - centerOS);
                float3 z_bs = normalize(cross(fixedAxisOS, camDirOS));
                float3 y_bs = normalize(cross(fixedAxisOS, z_bs));

                // matrix 
                float3x3 rotationMatrix = transpose(float3x3(fixedAxisOS, y_bs, z_bs));
                float3 posOS = mul(rotationMatrix, v.posOS.xyz);
                o.pos = TransformObjectToHClip(posOS);

                o.normalWS = mul(rotationMatrix ,TransformObjectToWorldDir(v.normalOS));
                o.posWS = TransformObjectToWorld(v.posOS).xyz;

                o.uvs0 = float4(v.uv * _MainTex_ST.xy + _MainTex_ST.zw, v.uv * _Beam_ST.xy + _Beam_ST.zw);
                o.uvs1 = float4(v.uv * _Noise_ST.xy + _Noise_ST.zw, v.uv * _Dirt_ST.xy + _Dirt_ST.zw);
                o.uv = v.uv; 
                o.posOS = v.posOS.xyz;
=======
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uvs0 = float4(v.uv * _MainTex_ST.xy + _MainTex_ST.zw, v.uv * _Beam_ST.xy + _Beam_ST.zw);
                o.uvs1 = float4(v.uv * _Noise_ST.xy + _Noise_ST.zw, v.uv * _Dirt_ST.xy + _Dirt_ST.zw);
                o.uv = v.uv;
                o.posOS = v.posOS;
>>>>>>> origin/main
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

<<<<<<< HEAD
                // Fade-out closing to grazing angle
                float3 camToPixelDirWS = normalize(i.posWS - _WorldSpaceCameraPos);
                float fade = abs(dot(i.normalWS, camToPixelDirWS));
                fade = smoothstep(0.1, 1.0, fade);

                half3 col = _BeamCol * gNoise + _DirtCol * gDirt;
                return half4(col, gBeam * fade);
=======
                half3 col = _BeamCol * gNoise + _DirtCol * gDirt;
                return half4(col, gBeam);
>>>>>>> origin/main
            }
            ENDHLSL
        }
    }
}
