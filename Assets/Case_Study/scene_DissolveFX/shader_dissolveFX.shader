Shader "Common/dissolveFX"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _DissolveTex ("Dissolve Texture", 2D) = "black" {}
        _RimCol ("Rim Color", Color) = (1,1,1,1)
        _RimThickness ("Rim Thickness", Range(0, 1)) = 0.1
        _RimSmoothness ("Rim Smoothness", Range(0, 1)) = 0.1
        _DissolveThreshold ("Dissolve Threshold", Range(0, 1)) = 0
        _TestFac ("TestFac", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="AlphaTest" "IgnoreProjector"="True"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 posWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _DissolveTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            sampler2D _DissolveTex;

            float4 _TestFac;
            float4 _RimCol;
            float _RimThickness;
            float _RimSmoothness;
            float _DissolveThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _DissolveTex);
                o.posWS = TransformObjectToWorld(v.posOS.xyz);
                o.normalWS = TransformObjectToWorldDir(v.normalOS);
                return o;
            }

            half remap(half value, half inputMin, half inputMax, half outputMin, half outputMax)
            {
                return lerp(outputMin, outputMax, saturate((value - inputMin) / (inputMax - inputMin)));
            }

            half4 frag (v2f i) : SV_Target
            {
                int addLightsCount = GetAdditionalLightsCount();
                half3 addLightCol; 
                for (int idx = 0; idx < addLightsCount; idx++)
                {
                    Light addlight = GetAdditionalLight(idx, i.posWS);
                    float3 lightDir = addlight.direction - i.posWS;
                    float NoL = saturate(dot(i.normalWS, lightDir)) + 0.00001;
                    addLightCol.rgb += addlight.color * addlight.distanceAttenuation * addlight.shadowAttenuation * NoL;
                }
                
                half3 col = 0;
                col += addLightCol;

                float3 posOS = TransformWorldToObject(i.posWS);
                float gradient = posOS.z * 0.5 + 0.5;

                half dissolveTex = tex2D(_DissolveTex, i.uv).r;

                _DissolveThreshold = remap(_DissolveThreshold, 0, 1, -1.1, 0.6);
                half dissolve = _DissolveThreshold + gradient;
                
                clip(dissolveTex - dissolve);
                half3 rimCol = _RimCol.rgb * (1 - smoothstep(0, _RimSmoothness ,dissolveTex - dissolve - _RimThickness));
                col += rimCol;
                

                return half4(col, 1);
            }
            ENDHLSL
        }

    }
}
