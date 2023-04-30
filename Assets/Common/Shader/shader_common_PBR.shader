Shader "Common/PBR"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        _MetallicTex ("Metallic Map", 2D) = "black" {}
        _RoughnessTex ("Roughness Map", 2D) = "white" {}
        _RoughnessScale ("Roughness Scale", Range(0, 1)) = 1.0
        _AOTex ("AO Map", 2D) = "white" {}
        _AOScale ("AO Scale", Range(0, 3)) = 1.0

        _TestFactor ("Test Factor", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}
        LOD 100

        Pass
        {
            Name "UniversalForward"
            Tags{ "LightMode" = "UniversalForward" }
            

            HLSLPROGRAM
            #pragma vertex vert_PBR
            #pragma fragment frag_PBR
        
            #pragma multi_compile_fwdbase
            
            // SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature _ALPHATEST_ON


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "K_PBRSetup.hlsl"
            #include "K_BRDF.hlsl"

            half4 frag_PBR (v2f_PBR i) : SV_Target
            {
                // === VARIABLE PREPARATION === //
                PBR_setup pbr_setup;
                SetupPBRVariables(i, pbr_setup);

                PBR_vectors pbr_vectors;
                SetupPBRVectors(i, pbr_vectors);

                // Get direct light info
                Light mainLight = GetMainLight(i.shadowCoord);
                
                // === DIRECT LIGHT === //
                // Specular // 
                // BRDF - Cook-Torrance 
                float3 k_s_DL = GetF_Slk(pbr_setup.F0.r, max(pbr_vectors.hv,0));
                float D = GetD_GGX(pbr_vectors.nh, pbr_setup.roughness);
                float G = GetG_SlkGGX(pbr_vectors.nv, pbr_vectors.nl, pbr_setup.roughness);
                float3 BRDF_spec_DL = D * G * k_s_DL / ( 4*pbr_vectors.nv*pbr_vectors.nl );
                // L_o
                half3 SpecCol_DL = BRDF_spec_DL * mainLight.color * pbr_vectors.nl * PI;   // to compensate for PI igonred in diffuse part
                
                // Diffuse - Lambert // 
                float3 k_d_DL = (1 - k_s_DL) * (1 - pbr_setup.metallic);
                float3 DiffCol_DL = k_d_DL * pbr_setup.baseCol * mainLight.color * pbr_vectors.nl;
                // Color from direct light
                float3 DirectCol = DiffCol_DL + SpecCol_DL;

                // === INDIRECT LIGHT === //
                // Specular //
                // Li - IBL 
                float3 IBLCol = GetIBL(pbr_vectors.n, pbr_vectors.v, pbr_setup.roughness);
                // BRDF - Cook-Torrance
                float3 k_s_IDL = GetF_SL(max(pbr_vectors.nv, 0), pbr_setup.F0, pbr_setup.roughness);
                float3 BRDF_spec_IDL = GetBRDF_specular_IDL(pbr_setup.roughness, pbr_setup.smoothness, BRDF_spec_DL, pbr_setup.F0, pbr_vectors.nv);
                // Specular
                float3 SpecCol_IDL = IBLCol * BRDF_spec_IDL * PI;   // to compensate for PI igonred in diffuse part

                // Diffuse - Lambert // 
                // Li - SH
                float3 SHCol = SampleSH(pbr_vectors.n);
                // BRDF - Lambert
                float k_d_IDL = (1 - k_s_IDL) * (1 - pbr_setup.metallic);
                half3 DiffCol_IDL = SHCol * k_d_IDL * pbr_setup.baseCol;
                // Color from indirect light
                half3 IndirectCol = SpecCol_IDL + DiffCol_IDL;

                // === FINAL COLOR RESULT === //
                // Shadow // 
                half shadow = mainLight.shadowAttenuation;

                half3 col = DirectCol * shadow * pbr_setup.AO + IndirectCol;

                half alpha = pbr_setup.alpha;
                return half4(col, alpha);
            }
            
            ENDHLSL
        }
       
       Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Front

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}