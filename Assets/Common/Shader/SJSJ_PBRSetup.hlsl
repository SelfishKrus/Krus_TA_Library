#ifndef SJSJ_PBRSETUP_INCLUDED
#define SJSJ_PBRSETUP_INCLUDED

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

    sampler2D _BaseTex;
    sampler2D _NormalTex;
    sampler2D _MetallicTex;
    sampler2D _RoughnessTex;
    sampler2D _AOTex;

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
        half3 nTS;  // normal tangent space
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


    // === FUNCTIONS === //
    void SetupPBRVariables(v2f_PBR i, inout PBR_setup pbrSetup) {
        
        // Masks decompression
        // r - metallic, g - AO, b - roughness
        pbrSetup.metallic = tex2D(_MetallicTex, i.uv).r;
        pbrSetup.AO = clamp(tex2D(_AOTex,i.uv).r, 0, 1);
        pbrSetup.AO = pow(pbrSetup.AO, _AOScale);
        pbrSetup.tempRoughness = tex2D(_RoughnessTex, i.uv).r;
        pbrSetup.tempRoughness = (pbrSetup.tempRoughness+_RoughnessScale) / (1+_RoughnessScale);
        pbrSetup.roughness = pow(pbrSetup.tempRoughness, 2);
        pbrSetup.smoothness = 1 - pbrSetup.tempRoughness;
        
        half4 texCol = tex2D(_BaseTex, i.uv);
        pbrSetup.baseCol = texCol.rgb;
        pbrSetup.alpha = texCol.a;
        pbrSetup.baseCol *= _Tint.rgb;
        pbrSetup.F0 = lerp(0.04, pbrSetup.baseCol, pbrSetup.metallic);

        return;
    }

    void SetupPBRVectors(v2f_PBR i, inout PBR_vectors pbrVectors) {

        float3 posWS = float3(i.TBN0.w, i.TBN1.w, i.TBN2.w);

        half4 normalCol = tex2D(_NormalTex, i.uv);
        half3 normalTS = UnpackNormalScale(normalCol, _NormalScale);
        half3 normalWS = normalize(half3(dot(i.TBN0.xyz, normalTS), dot(i.TBN1.xyz, normalTS), dot(i.TBN2.xyz, normalTS)));

        Light mainLight = GetMainLight(i.shadowCoord);

        pbrVectors.nTS = normalTS;
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


#endif