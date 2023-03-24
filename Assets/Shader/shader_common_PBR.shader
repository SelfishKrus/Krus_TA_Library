Shader "Common/PBR"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        [Gamma] _MetallicTex ("Metallic Map", 2D) = "black" {}
        _RoughnessTex ("Roughness Map", 2D) = "white" {}
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
            #pragma vertex vert
            #pragma fragment frag
            // fog
            #pragma multi_compile_fog
        
            #pragma multi_compile_fwdbase
            
            // SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature _ALPHATEST_ON


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 normal : NORMAL;
                half4 tangent : TANGENT;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 TBN0 : TEXCOORD1;
                float4 TBN1 : TEXCOORD2;
                float4 TBN2 : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseTex_ST;
            half _NormalScale;
            float _AOScale;
            float4 _Tint;

            float _TestFactor;
            CBUFFER_END

            TEXTURE2D(_BaseTex);        SAMPLER(sampler_BaseTex);
            TEXTURE2D(_NormalTex);      SAMPLER(sampler_NormalTex);
            TEXTURE2D(_MetallicTex);    SAMPLER(sampler_MetallicTex);
            TEXTURE2D(_RoughnessTex);   SAMPLER(sampler_RoughnessTex);
            TEXTURE2D(_AOTex);          SAMPLER(sampler_AOTex);
            
            // FUNCTION /////////////////////////////////////////////////////////////////
            // NDF - GGX 
            float GetD(float nh, float roughness) 
            {
                half a2 = roughness * roughness;
                half nh2 = nh * nh;
                half nom = a2;
                half denom = nh2 * (a2 - 1) + 1;
                denom = denom * denom * PI;
                return nom / denom;
            }

            // Geometry Function - Schlick-GGX
            float GetG(float nv, float nl, float roughness)
            {
                float k = pow(roughness+1, 2) * 0.125;
                float G_in = nl / lerp(nl, 1, k);
                float G_out = nv / lerp(nv, 1, k);
                return G_in * G_out;
            }

            // Fresnel_direct_light - UE Schilick 
            // hv
            float3 GetF_direct(float F0, float hv)
            {
                float Fre = exp2((-5.55473*hv - 6.98316) * hv);
                return lerp(Fre, 1, F0);
            }
            
            // Fresnel_indirect_light - Sébastien Lagarde
            // nv
            float3 GetF_indirect(float cosTheta, float3 F0, float roughness)
            {
                return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
            }

            // Li - indirect light - specular
            // IBL
            float3 GetLi_indirect_specular(float3 worldNormal, float3 viewDir, float roughness)
            {
                float3 reflectViewDir = reflect(-viewDir, worldNormal);
                // curve fitting
                roughness = roughness * (1.7 - 0.7 * roughness);
                // sample the cubemap at different mip levels based on roughness
                float mipLevel = roughness * 6;
                float4 specCol = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectViewDir, mipLevel);
                
                #if !defined(UNITY_USE_NATIVE_HDR)
                return DecodeHDREnvironment(specCol, unity_SpecCube0_HDR);
                #else 
                return specCol.xyz;
                #endif

            }
            
            // approximate BRDF - indirect light - specular
            // unity fitting curve instead of UE LUT
            float3 GetBRDF_indirect_specular(float roughness, float smoothness, float3 BRDF_spec, float3 F0, float nv)
            {
                #ifdef UNITY_COLORSPACE_GAMMA
                float SurReduction = 1-0.28*roughness;
                #else
                float SurReduction = 1 / (roughness*roughness+1);
                #endif

                #if defined(SHADER_API_GLES)
                float Reflectivity = BRDF_spec.x;
                #else
                float Reflectivity = max(max(BRDF_spec.x,BRDF_spec.y),BRDF_spec.z);
                #endif

                half GrazingTSection = saturate(Reflectivity+smoothness);
                float Fre = Pow4(1-nv);


                return lerp(F0,GrazingTSection,Fre)*SurReduction;
            }
            
            // Li - direct light - diffuse
            // SH
            float3 GetLi_indirect_diffuse(float3 worldNormal)
            {
                real4 SHCoefficients[7];
                SHCoefficients[0] = unity_SHAr;
                SHCoefficients[1] = unity_SHAg;
                SHCoefficients[2] = unity_SHAb;
                SHCoefficients[3] = unity_SHBr;
                SHCoefficients[4] = unity_SHBg;
                SHCoefficients[5] = unity_SHBb;
                SHCoefficients[6] = unity_SHC;

                float3 col = SampleSH9(SHCoefficients,worldNormal);
                
                return max(0, col);
            }
            

            // FUNCTION END /////////////////////////////////////////////////////////////////

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseTex);

                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                half3 worldNormal = TransformObjectToWorldNormal(v.normal.xyz, true);
                half3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz, true);
                half3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

                // TBN matrix
                o.TBN0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TBN1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TBN2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                
                // shadowmap sampling coordinate
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.shadowCoord = GetShadowCoord(vertexInput);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // VARIABLE PREPARATION ////////////////////////////////////////////////////
                // world normal
                half4 normalCol = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);
                half3 tangentNormal = UnpackNormalScale(normalCol, _NormalScale);
                half3 worldNormal = normalize(half3(dot(i.TBN0.xyz, tangentNormal), dot(i.TBN1.xyz, tangentNormal), dot(i.TBN2.xyz, tangentNormal)));
                
                // Masks decompression
                // r - metallic, g - AO, b - roughness
                half metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, i.uv).r;
                half AO = clamp(SAMPLE_TEXTURE2D(_AOTex, sampler_AOTex, i.uv).r, 0, 1);
                AO = pow(AO, _AOScale);
                half tempRoughness = SAMPLE_TEXTURE2D(_RoughnessTex, sampler_RoughnessTex, i.uv).r;
                half roughness = pow(tempRoughness, 2);
                half smoothness = 1 - tempRoughness;
                
                half3 baseCol = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv).rgb;
                half alpha = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv).a;
                baseCol *= _Tint.rgb;
                half3 F0 = lerp(0.04, baseCol, metallic);

                float3 worldPos = float3(i.TBN0.w, i.TBN1.w, i.TBN2.w);
                Light mainLight = GetMainLight(i.shadowCoord);
                float3 n = worldNormal;
                float3 l = normalize(mainLight.direction);
                float3 v = SafeNormalize(_WorldSpaceCameraPos - worldPos);
                float3 h = normalize(v+l);
                float nv = max(saturate(dot(n, v)), 0.00001);
                float nl = max(saturate(dot(n, l)), 0.00001);
                float hv = max(saturate(dot(h, v)), 0.00001);
                float nh = max(saturate(dot(n, h)), 0.00001);
                float lh = max(saturate(dot(l, h)), 0.00001);
                
                // DIRECT LIGHT ////////////////////////////////////////////////////
                // Specular - Cook-Torrance
                float D = GetD(nh, roughness);
                float G = GetG(nv, nl, roughness);
                float3 F_direct = GetF_direct(F0.r, max(hv,0));
                float3 BRDF_direct_spec = D * G * F_direct / ( 4*nv*nl );
                float3 SpecCol_direct = BRDF_direct_spec * mainLight.color * nl * PI;   // to compensate for PI igonred in diffuse part
                // Diffuse - Lambert
                float3 k_s_direct = F_direct;
                float3 k_d_direct = (1 - k_s_direct) * (1 - metallic);
                float3 DiffCol_direct = k_d_direct * baseCol * mainLight.color * nl;
                // Color from direct light
                float3 DirectCol = DiffCol_direct + SpecCol_direct;

                // INDIRECT LIGHT ////////////////////////////////////////////////////
                float3 F_indirect = GetF_indirect(max(hv, 0), F0, roughness);
                float k_s_indirect = F_indirect.r;
                float k_d_indirect = (1 - k_s_indirect) * (1 - metallic);
                // Specular - Cook-Torrance
                float3 Li_indirect_specular = GetLi_indirect_specular(n, v, roughness);
                float3 BRDF_indirect_spec = GetBRDF_indirect_specular(roughness, smoothness, BRDF_direct_spec, F0, nv);
                float3 SpecCol_indirect = k_s_indirect * Li_indirect_specular * BRDF_indirect_spec * PI; 
                // Diffuse - Lambert
                float3 Li_indirect_diffuse = GetLi_indirect_diffuse(worldNormal);
                float3 DiffCol_indirect = Li_indirect_diffuse * k_d_indirect * baseCol;
                // Color from indirect light
                float3 IndirectCol = SpecCol_indirect + DiffCol_indirect;

                // FINAL COLOR RESULT ////////////////////////////////////////////////////
                // shadow
                float3 shadow = mainLight.shadowAttenuation;

                float3 finalCol = DirectCol * shadow * AO + IndirectCol;

                half4 col = 1; 
                col.rgb = finalCol;
                col.a = alpha;
                return col;
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