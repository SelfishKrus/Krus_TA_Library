Shader "CharacterRendering/Skin_P3S"
{
    Properties
    {
        // PBR.hlsl Properties
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        _MetallicTex ("Metallic Map", 2D) = "black" {}
        _RoughnessTex ("Roughness Map", 2D) = "white" {}
        _RoughnessScale ("Roughness Scale", Range(0, 5)) = 1.0
        _AOTex ("AO Map", 2D) = "white" {}
        _AOScale ("AO Scale", Range(0, 3)) = 1.0


    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert_PBR
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Common/Shader/SJSJPBR.hlsl"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 TBN0 : TEXCOORD1;
                float4 TBN1 : TEXCOORD2;
                float4 TBN2 : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            half4 frag (v2f i) : SV_Target
            {
                // === VARIABLE PREPARATION === //
                PBR_setup pbrSetup;
                SetupPBRVariables(i, pbrSetup);

                PBR_vectors pbrVectors;
                SetupPBRVectors(i, pbrVectors);

                // Get direct light info
                Light mainLight = GetMainLight(i.shadowCoord);

                // === DIRECT LIGHT === //
                // Specular // 
                // BRDF - Cook-Torrance 
                float3 k_s_DL = GetF_DL(pbrSetup.F0.r, max(pbrVectors.hv,0));
                float D = GetD(pbrVectors.nh, pbrSetup.roughness);
                float G = GetG(pbrVectors.nv, pbrVectors.nl, pbrSetup.roughness);
                float3 BRDF_spec_DL = D * G * k_s_DL / ( 4*pbrVectors.nv*pbrVectors.nl );
                // L_o
                half3 SpecCol_DL = BRDF_spec_DL * mainLight.color * pbrVectors.nl * PI;   // to compensate for PI igonred in diffuse part
                
                // Diffuse - Lambert // 
                float3 k_d_DL = (1 - k_s_DL) * (1 - pbrSetup.metallic);
                float3 DiffCol_DL = k_d_DL * pbrSetup.baseCol * mainLight.color * pbrVectors.nl;
                // Color from direct light
                float3 DirectCol = DiffCol_DL + SpecCol_DL;
                
                half3 col = DirectCol * pbrSetup.AO;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
