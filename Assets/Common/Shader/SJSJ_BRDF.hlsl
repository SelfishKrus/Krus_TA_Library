#ifndef SJSJ_BRDF_INCLUDED
#define SJSJ_BRDF_INCLUDED


    // PBR ITEMS //////////////////////////////////////////////////////////////////////
    // NDF - GGX 
    float GetD_GGX(float nh, float roughness) 
    {
        half a2 = roughness * roughness;
        half nh2 = nh * nh;
        half nom = a2;
        half denom = nh2 * (a2 - 1) + 1;
        denom = denom * denom * PI;
        return nom / denom;
    }

    // Geometry Function - Schlick-GGX
    float GetG_SlkGGX(float nv, float nl, float roughness)
    {
        float k = pow(roughness+1, 2) * 0.125;
        float G_in = nl / lerp(nl, 1, k);
        float G_out = nv / lerp(nv, 1, k);
        return G_in * G_out;
    }

    // Fresnel_direct_light - UE Schilick 
    // hv
    float3 GetF_Slk(float F0, float hv)
    {
        float Fre = exp2((-5.55473*hv - 6.98316) * hv);
        return lerp(Fre, 1, F0);
    }
    
    // Fresnel_indirect_light - Sébastien Lagarde
    // nv
    float3 GetF_SL(float cosTheta, float3 F0, float roughness)
    {
        return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
    }

    // Li - indirect light - specular
    // IBL
    float3 GetIBL(float3 normalWS, float3 viewDir, float roughness)
    {
        float3 reflectViewDir = reflect(-viewDir, normalWS);
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
    float3 GetBRDF_specular_IDL(float roughness, float smoothness, float3 BRDF_spec, float3 F0, float nv)
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

    // Specular BRDF - Kelemen/Szirmay-Kalos // 
    half3 EvaluateSpecular_KS(
        sampler2D _LUT_BeckmannNDF,
        half nh,
        half roughness,
        half3 fresnel,
        half3 h,
        half3 lightCol,
        half nl,
        half lh,
        inout half3 BRDF_spec_DL
    ) {
        half3 specCol;

        // NDF - Beckmann
        half2 uvBeckmannNDF = {nh, roughness};
        half D = pow(2 * max(tex2D(_LUT_BeckmannNDF, uvBeckmannNDF).r, 0), 10);
        half3 frSpec = D * fresnel / ( 4 * dot(h, h));
        BRDF_spec_DL = frSpec;
        // frSpec = D * fresnel.r / ( 4 * lh * lh);
        half3 Li = lightCol * nl;

        specCol = Li * frSpec;

        return specCol;
    }

#endif