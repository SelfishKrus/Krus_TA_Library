Shader "CharacterRendering/Skin_PI3S"
{
    Properties
    {
        // pbr_setup Properties
        [Header(Common PBR Setup)]
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        [NoScaleOffset] _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        _NormalBlurredBias ("Normal Blurred Bias", Range(0, 10)) = 3
        [NoScaleOffset] _MetallicTex ("Metallic Map", 2D) = "black" {}
        [NoScaleOffset] _RoughnessTex ("Roughness Map", 2D) = "white" {}
        _RoughnessScale ("Roughness Scale", Range(0, 5)) = 1.0
        [NoScaleOffset] _AOTex ("AO Map", 2D) = "white" {}
        _AOScale ("AO Scale", Range(0, 3)) = 1.0
        [Space(50)]

        [Header(BSSRDF Properties)]
        [NoScaleOffset] _MasksTex ("Masks - R(Curvature) G(AO) B(Thickness)", 2D) = "" {}
        [NoScaleOffset] _SSLUT_Curvature ("Curvature LUT", 2D) = "" {}
        _CurvatureBias ("Curvature Bias", Vector) = (1,0,0,0)
        [NoScaleOffset] _LUT_BeckmannNDF ("Beckmann NDF LUT", 2D) = "" {}
        [Space(50)]

        [Header(BTDF Properties)]
        _TransmissionCol ("Transmission Color", Color) = (1,0.5,0.5,1)
        _ThicknessScale ("Thickness Scale", Range(0, 1)) = 0.01
        _Distortion ("Transmission Distortion", Range(0, 1)) = 1
        [Space(50)]

        [Header(Tone Mapping)]
        _Exposure ("Exposure", Range(0, 5)) = 1.0
        [Toggle(_ENABLE_TONEMAPPING)] _EnableTonemapping ("Enable Tonemapping", Float) = 1

        [Space(50)]
        [Toggle(_ENABLE_RECEIVE_SHADOW)] _EnableReceiveShadow ("Receive Shadow", Float) = 1
        _ShadowExposure ("Shadow Bias", Range(0, 1)) = 0.5
        _Test ("Test", Vector) = (1,0,0,0)
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
            
            #pragma multi_compile_fwdbase

            // SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ENABLE_RECEIVE_SHADOW

            #pragma multi_compile _ _ENABLE_TONEMAPPING


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/Common/Shader/SJSJ_PBRSetup.hlsl"
            #include "Assets/Common/Shader/SJSJ_BRDF.hlsl"
            #include "Assets/Common/Shader/SJSJ_BSSRDF.hlsl"
            #include "Assets/Common/Shader/SJSJ_BTDF.hlsl"
            #include "Assets/Common/Shader/Tonemapping.hlsl"


            half _NormalBlurredBias;
            half2 _CurvatureBias;

            sampler2D _MasksTex;
            sampler2D _SSLUT_Curvature;
            sampler2D _LUT_BeckmannNDF;

            half4 _TransmissionCol;
            half _Distortion;
            half _ThicknessScale;

            half _Exposure;

            int _EnableReceiveShadow;
            half _ShadowExposure;
            float4 _Test;


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
                // for common PBR
                PBR_setup pbr_setup;
                SetupPBRVariables(i, pbr_setup);
                PBR_vectors pbr_vectors;
                SetupPBRVectors(i, pbr_vectors);

                // for BSSRDF
                half3 masks = tex2D(_MasksTex, i.uv).rgb;
                half curvature = masks.r;
                half thickness = masks.b;
                thickness = (thickness + _ThicknessScale) / ( 1 + _ThicknessScale);

                half3 normalTS = pbr_vectors.nTS;
                half3 normalBlurredTS = UnpackNormalScale(tex2Dbias(_NormalTex, half4(i.uv, 0, _NormalBlurredBias)), _NormalScale);
                half3 normalBlurredWS = normalize(
                      half3(dot(i.TBN0.xyz, normalBlurredTS), 
                            dot(i.TBN1.xyz, normalBlurredTS), 
                            dot(i.TBN2.xyz, normalBlurredTS)));

                // direct light
                Light mainLight = GetMainLight(i.shadowCoord);

                // === Direct Light === //
                half3 k_s_DL = GetF_SL(pbr_vectors.hv, pbr_setup.F0, pbr_setup.roughness);
                half3 k_d_DL = 1 - k_s_DL;
                // BSSRDF // 
                half3 diffCol_DL = EvaluatePISSS(
                    pbr_vectors.n, 
                    normalBlurredWS, 
                    pbr_vectors.l, 
                    curvature,
                    mainLight.color,
                    pbr_setup.baseCol,
                    _SSLUT_Curvature,
                    _CurvatureBias,
                    k_d_DL
                    );
                // Specular //
                half3 BRDF_spec_DL;
                half3 specCol_DL = EvaluateSpecular_KS(
                    _LUT_BeckmannNDF,
                    pbr_vectors.nh,
                    pbr_setup.roughness,
                    k_s_DL,
                    pbr_vectors.h,
                    mainLight.color,
                    pbr_vectors.nl,
                    pbr_vectors.lh, 
                    BRDF_spec_DL
                    );

                // BTDF //
                half3 transCol_DL = EvaluateTransmission_CM(
                    pbr_vectors.l,
                    normalBlurredWS,
                    _Distortion,
                    pbr_vectors.v,
                    thickness,
                    _TransmissionCol,
                    mainLight.color
                );
                
                half3 col_DL = diffCol_DL + specCol_DL + transCol_DL;

                // === Indirect Light === //
                // Specular - Cook Torrance - Curve fitting //
                float3 IBLCol = GetIBL(pbr_vectors.n, pbr_vectors.v, pbr_setup.roughness);
                // BRDF - Cook-Torrance
                // k_s_IDL not used here cuz it's already included in BRDF_spec_IDL
                float3 k_s_IDL = GetF_SL(max(pbr_vectors.nv, 0), pbr_setup.F0, pbr_setup.roughness);
                float3 BRDF_spec_IDL = GetBRDF_specular_IDL(pbr_setup.roughness, pbr_setup.smoothness, BRDF_spec_DL, pbr_setup.F0, pbr_vectors.nv);
                // Specular
                float3 SpecCol_IDL = IBLCol * BRDF_spec_IDL * PI; 

                // Diffuse - Lambert // 
                // Li - SH
                float3 SHCol = SampleSH(pbr_vectors.n);
                // BRDF - Lambert
                float k_d_IDL = (1 - k_s_IDL) * (1 - pbr_setup.metallic);
                half3 DiffCol_IDL = SHCol * k_d_IDL * pbr_setup.baseCol;

                half3 col_IDL = DiffCol_IDL + SpecCol_IDL;

                // === FINAL RESULT === //
                #if _ENABLE_RECEIVE_SHADOW
                    half shadow = saturate(mainLight.shadowAttenuation);
                    shadow = (shadow + _ShadowExposure) / (1 + _ShadowExposure); 
                #else
                    half shadow = 1;
                #endif

                half3 col = 1;
                col = col_DL * shadow * pbr_setup.AO + col_IDL;
                #if _ENABLE_TONEMAPPING
                    col = Tonemapping_Filmic(col, _Exposure);
                #endif

                return half4(col, 1);

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
