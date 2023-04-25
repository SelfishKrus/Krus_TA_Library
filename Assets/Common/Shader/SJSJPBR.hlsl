#ifndef SJSJPBR_INCLUDED
#define SJSJPBR_INCLUDED

    // PBR VARIABLES PREPARATION ////////////////////////////////////////////////////

    // === VARIABLES DECLARATION === //
    CBUFFER_START(UnityPerMaterial)
    float4 _BaseTex_ST;
    half _NormalScale;
    half _RoughnessScale;
    float _AOScale;
    float4 _Tint;

    float _TestFactor;
    CBUFFER_END

    TEXTURE2D(_BaseTex);        SAMPLER(sampler_BaseTex);
    TEXTURE2D(_NormalTex);      SAMPLER(sampler_NormalTex);
    TEXTURE2D(_MetallicTex);    SAMPLER(sampler_MetallicTex);
    TEXTURE2D(_RoughnessTex);   SAMPLER(sampler_RoughnessTex);
    TEXTURE2D(_AOTex);          SAMPLER(sampler_AOTex);

    // === STRUCT === //

    // vert shader input
    struct appdata_PBR
    {
        float4 posOS : POSITION;
        float2 uv : TEXCOORD0;
        half4 normalOS : NORMAL;
        half4 tangentOS : TANGENT;
        
    };

    // frag shader input
    struct v2f_PBR
    {
        float2 uv : TEXCOORD0;
        float4 pos : SV_POSITION;
        float4 TBN0 : TEXCOORD1;
        float4 TBN1 : TEXCOORD2;
        float4 TBN2 : TEXCOORD3;
        float4 shadowCoord : TEXCOORD4;
    };
    
    // PBR setup variables
    struct PBR_setup
    {
        half metallic;
        half AO;
        half tempRoughness;
        half roughness;
        half smoothness;

        half3 baseCol;
        half alpha;

        half3 F0;
    };

    // PBR Vectors
    struct PBR_vectors
    {
        half3 n;    // normal
        half3 l;    // light direction
        half3 v;    // view direction
        half3 h;    // half vector

        half nv;    // dot(n, v)
        half nl;    // dot(n, l)
        half hv;    // dot(h, v)
        half nh;    // dot(n, h)
        half lh;    // dot(l, h)
    };

    // === FUNCTIONS === //
    void SetupPBRVariables(v2f_PBR i, inout PBR_setup pbrSetup) {
        
        // Masks decompression
        // r - metallic, g - AO, b - roughness
        pbrSetup.metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, i.uv).r;
        pbrSetup.AO = clamp(SAMPLE_TEXTURE2D(_AOTex, sampler_AOTex, i.uv).r, 0, 1);
        pbrSetup.AO = pow(pbrSetup.AO, _AOScale);
        pbrSetup.tempRoughness = SAMPLE_TEXTURE2D(_RoughnessTex, sampler_RoughnessTex, i.uv).r;
        pbrSetup.tempRoughness = saturate(pbrSetup.tempRoughness * _RoughnessScale);
        pbrSetup.roughness = pow(pbrSetup.tempRoughness, 2);
        pbrSetup.smoothness = 1 - pbrSetup.tempRoughness;
        
        half4 texCol = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv);
        pbrSetup.baseCol = texCol.rgb;
        pbrSetup.alpha = texCol.a;
        pbrSetup.baseCol *= _Tint.rgb;
        pbrSetup.F0 = lerp(0.04, pbrSetup.baseCol, pbrSetup.metallic);

        return;
    }

    void SetupPBRVectors(v2f_PBR i, inout PBR_vectors pbrVectors) {

        float3 posWS = float3(i.TBN0.w, i.TBN1.w, i.TBN2.w);

        half4 normalCol = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);
        half3 normalTS = UnpackNormalScale(normalCol, _NormalScale);
        half3 normalWS = normalize(half3(dot(i.TBN0.xyz, normalTS), dot(i.TBN1.xyz, normalTS), dot(i.TBN2.xyz, normalTS)));

        Light mainLight = GetMainLight(i.shadowCoord);

        pbrVectors.n = normalWS;
        pbrVectors.l = normalize(mainLight.direction);
        pbrVectors.v = SafeNormalize(_WorldSpaceCameraPos - posWS);
        pbrVectors.h = normalize(pbrVectors.v+pbrVectors.l);
        pbrVectors.nv = max(saturate(dot(pbrVectors.n, pbrVectors.v)), 0.00001);
        pbrVectors.nl = max(saturate(dot(pbrVectors.n, pbrVectors.l)), 0.00001);
        pbrVectors.hv = max(saturate(dot(pbrVectors.h, pbrVectors.v)), 0.00001);
        pbrVectors.nh = max(saturate(dot(pbrVectors.n, pbrVectors.h)), 0.00001);
        pbrVectors.lh = max(saturate(dot(pbrVectors.l, pbrVectors.h)), 0.00001);

        return;

    }

    // PBR ITEMS //////////////////////////////////////////////////////////////////////
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
    float3 GetF_DL(float F0, float hv)
    {
        float Fre = exp2((-5.55473*hv - 6.98316) * hv);
        return lerp(Fre, 1, F0);
    }
    
    // Fresnel_indirect_light - Sébastien Lagarde
    // nv
    float3 GetF_IDL(float cosTheta, float3 F0, float roughness)
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

    // BRDF - direct light - specular
    float3 GetBRDF_specular_DL(PBR_setup pbrSetup, PBR_vectors pbrVectors) {

        float D = GetD(pbrVectors.nh, pbrSetup.roughness);
        float G = GetG(pbrVectors.nv, pbrVectors.nl, pbrSetup.roughness);
        float3 F_direct = GetF_DL(pbrSetup.F0.r, max(pbrVectors.hv,0));
        float3 BRDF_direct_spec = D * G * F_direct / ( 4*pbrVectors.nv*pbrVectors.nl );

        return BRDF_direct_spec;

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
    
    // Li - indirect light - diffuse
    // SH
    float3 GetSH(float3 normalWS)
    {
        real4 SHCoefficients[7];
        SHCoefficients[0] = unity_SHAr;
        SHCoefficients[1] = unity_SHAg;
        SHCoefficients[2] = unity_SHAb;
        SHCoefficients[3] = unity_SHBr;
        SHCoefficients[4] = unity_SHBg;
        SHCoefficients[5] = unity_SHBb;
        SHCoefficients[6] = unity_SHC;

        float3 col = SampleSH9(SHCoefficients,normalWS);
        
        return max(0, col);
    }

    // PBR MAIN FUNCTIONS /////////////////////////////////////////////////////////

    // === Specular - Cook-Torrance - Direct Light === // 
    half3 GetSpecCol_DL(PBR_setup pbrSetup, PBR_vectors pbrVectors, Light mainLight) {
    
        float3 BRDF_direct_spec = GetBRDF_specular_DL(pbrSetup, pbrVectors);
        half3 SpecCol_direct = BRDF_direct_spec * mainLight.color * pbrVectors.nl * PI;   // to compensate for PI igonred in diffuse part
    
        return SpecCol_direct;
    }

    // === Diffuse - Lambert - Direct Light === //
    half3 GetDiffCol_DL(PBR_setup pbrSetup, PBR_vectors pbrVectors, Light mainLight) {
        
        float3 k_s_direct = GetF_DL(pbrSetup.F0.r, max(pbrVectors.hv,0));
        float3 k_d_direct = (1 - k_s_direct) * (1 - pbrSetup.metallic);
        float3 DiffCol_direct = k_d_direct * pbrSetup.baseCol * mainLight.color * pbrVectors.nl;
    
        return DiffCol_direct;
    }

    // === Specular - Cook-Torrance - Indirect Light === // 
    // IBL
    half3 GetSpecCol_IDL(PBR_setup pbrSetup, PBR_vectors pbrVectors, half3 Li_indirect_specular) {

        float3 k_s_indirect = GetF_IDL(max(pbrVectors.hv, 0), pbrSetup.F0, pbrSetup.roughness);
        float3 BRDF_direct_spec = GetBRDF_specular_DL(pbrSetup, pbrVectors);
        float3 BRDF_indirect_spec = GetBRDF_specular_IDL(pbrSetup.roughness, pbrSetup.smoothness, BRDF_direct_spec, pbrSetup.F0, pbrVectors.nv);
        float3 SpecCol_indirect = k_s_indirect * Li_indirect_specular * BRDF_indirect_spec * PI;

        return SpecCol_indirect; 

    }

    // === Diffuse - Lambert - Indirect Light === //
    // SH
    half3 GetDiffCol_IDL(PBR_setup pbrSetup, PBR_vectors pbrVectors, half3 Li_indirect_diffuse) {

        float F_indirect = GetF_IDL(max(pbrVectors.hv, 0), pbrSetup.F0, pbrSetup.roughness);
        float k_d_indirect = (1 - F_indirect) * (1 - pbrSetup.metallic);
        float3 DiffCol_indirect = Li_indirect_diffuse * k_d_indirect * pbrSetup.baseCol;
        
        return DiffCol_indirect;

    }


    // PBR VERTEX SHADER ////////////////////////////////////////////////////

    v2f_PBR vert_PBR (appdata_PBR v)
    {
        v2f_PBR o;
        o.pos = TransformObjectToHClip(v.posOS.xyz);
        o.uv = TRANSFORM_TEX(v.uv, _BaseTex);

        float3 posWS = TransformObjectToWorld(v.posOS.xyz);
        half3 normalWS = TransformObjectToWorldNormal(v.normalOS.xyz, true);
        half3 tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz, true);
        half3 binormalWS = cross(normalWS, tangentWS) * v.tangentOS.w;

        // TBN matrix
        o.TBN0 = float4(tangentWS.x, binormalWS.x, normalWS.x, posWS.x);
        o.TBN1 = float4(tangentWS.y, binormalWS.y, normalWS.y, posWS.y);
        o.TBN2 = float4(tangentWS.z, binormalWS.z, normalWS.z, posWS.z);
        
        // shadowmap sampling coordinate
        VertexPositionInputs vertexInput = GetVertexPositionInputs(v.posOS.xyz);
        o.shadowCoord = GetShadowCoord(vertexInput);

        return o;
    }

    // PBR FRAGMENT SHADER ////////////////////////////////////////////////////

    half4 frag_PBR (v2f_PBR i) : SV_Target
    {
        // === VARIABLE PREPARATION === //
        PBR_setup pbrSetup;
        SetupPBRVariables(i, pbrSetup);

        PBR_vectors pbrVectors;
        SetupPBRVectors(i, pbrVectors);

        // Get direct light info
        Light mainLight = GetMainLight(i.shadowCoord);
        
        // === DIRECT LIGHT === //
        // Get specular color from direct light
        half3 SpecCol_direct = GetSpecCol_DL(pbrSetup, pbrVectors, mainLight);
        // Diffuse - Lambert
        half3 DiffCol_direct = GetDiffCol_DL(pbrSetup, pbrVectors, mainLight);
        // Color from direct light
        float3 DirectCol = DiffCol_direct + SpecCol_direct;

        // === INDIRECT LIGHT === //
        // Specular - Cook-Torrance
        float3 IBLCol = GetIBL(pbrVectors.n, pbrVectors.v, pbrSetup.roughness);
        half3 SpecCol_indirect = GetSpecCol_IDL(pbrSetup, pbrVectors, IBLCol);
        // Diffuse - Lambert
        float3 SHCol = GetSH(pbrVectors.n);
        half3 DiffCol_indirect = GetDiffCol_IDL(pbrSetup, pbrVectors, SHCol);
        // Color from indirect light
        float3 IndirectCol = SpecCol_indirect + DiffCol_indirect;

        // === FINAL COLOR RESULT === //
        // shadow
        float3 shadow = mainLight.shadowAttenuation;

        float3 finalCol = DirectCol * shadow * pbrSetup.AO + IndirectCol;

        half4 col = 1; 
        col.rgb = finalCol;
        col.a = pbrSetup.alpha;
        return col;
    }

#endif