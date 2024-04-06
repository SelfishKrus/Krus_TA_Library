Shader "Krus/StylizedWater"
{
    Properties
    {
        _WaterNoiseTex ("Water Noise Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Float) = 1

        [Header(SSS)]
        _WaterColor ("Water Color", Color) = (1, 1, 1, 1)
        _Speed ("Speed", Float) = 1
        _WaterTurbidity ("Water Turbidity", Range(0,10)) = 1

        [Header(BRDF)]
        _TestFac ("Test Factor", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent-10"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/Common/Shader/K_Pbr.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 tbn_r0 : TEXCOORD1;
                float4 tbn_r1 : TEXCOORD2;
                float4 tbn_r2 : TEXCOORD3;
                float4 pos : SV_POSITION;
            };

            #include "Assets/Common/Shader/K_Utility.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _WaterNoiseTex_ST;
            float4 _TestFac;
            float3 _WaterColor;
            float _WaterTurbidity;
            float _Speed;

            float _NormalScale;
            CBUFFER_END

            TEXTURE2D(_WaterNoiseTex);    SAMPLER(sampler_WaterNoiseTex);
            TEXTURE2D(_NormalMap);    SAMPLER(sampler_NormalMap);

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.pos = TransformObjectToHClip(IN.posOS.xyz);
                OUT.uv = IN.uv; 

                // tbn 
                EncodeTbn(IN, OUT);

                return OUT;
            }

            half4 frag (v2f IN) : SV_Target            
            {   


                // PRE // 
                float time = _Time.y * _Speed * 0.1;
                // dir 
                Light mainLight = GetMainLight();

                TbnData tbnData;
                DecodeTbn(IN, tbnData);
                float3 normalTS_detail = UnpackNormalTS(TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), IN.uv + time, _NormalScale);
                float3 normalWS_detail = mul(tbnData.m_TS2WS, normalTS_detail);

                float3 viewDirWS = normalize(_WorldSpaceCameraPos - tbnData.posWS);
                float3 H = normalize(viewDirWS + mainLight.direction);
                float NoV = saturate(dot(viewDirWS, normalWS_detail));
                float NoH = saturate(dot(normalWS_detail, H));

                float2 uvSS = IN.pos / _ScaledScreenParams.xy;

                float waterNoise = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv + time).r;
                waterNoise = waterNoise * 2 - 1;
                float fresnel = Fresnel_SchlickRoughness(NoV, 0.04f, 0.9f);

                // REFRACTION //
                // flow FX
                float2 uvSS_water = saturate(uvSS + waterNoise * _NormalScale);
                // water depth
                float sceneDepth = SampleSceneDepth(uvSS_water);
                float3 scenePosWS = ComputeWorldSpacePosition(uvSS, sceneDepth, UNITY_MATRIX_I_VP);
                float waterFade = saturate(distance(scenePosWS, tbnData.posWS) / 50);
                waterFade = saturate(waterFade * _WaterTurbidity);
                // blend
                float3 sceneColor = SampleSceneColor(uvSS_water);
                float3 refracCol = lerp(sceneColor, _WaterColor, waterFade);

                // REFLECTION // 
                // env
                float3 reflDirWS = reflect(-viewDirWS, normalWS_detail);
                float3 reflecCol_Env = CalculateIrradianceFromReflectionProbes(reflDirWS, tbnData.posWS, 0.0f);

                float3 reflecCol = reflecCol_Env;

                half3 col = lerp(refracCol, reflecCol, fresnel);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
