#ifndef HAIR_MAIN_INCLUDED
#define HAIR_MAIN_INCLUDED

    half3 _SpecCol;
    sampler2D _ShiftTangentTex;
    half2 _Shift;
    half2 _Glossiness;

    half _Alpha;
    half _ShadowIntensity;

    struct appdata
    {
        float4 posOS : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 pos : SV_POSITION;
    };

    // FUNCTION START ////////////////////////////////////////////////

    half StrandSpecular (half3 t, half3 v, half3 l, half exponent) {
        half3 h = normalize(l + v);
        half th = dot(t, h);
        half sin_th = sqrt(1 - th * th);
        half dirAtten = smoothstep(-1, 0, th);

        return dirAtten * pow(sin_th, exponent);
    }

    half3 ShiftTangent (half3 t, half3 n, half shift) {
        half3 shiftedT = t + shift * n;
        return normalize(shiftedT);
    }

    // FUNCTION END /////////////////////////////////////////////////

    // PRE-Z FRAG //
    half4 frag_preZ (v2f_PBR i) : SV_Target
    {
        // === VARIABLE PREPARATION === //
        PBR_setup pbr_setup;
        SetupPBRVariables(i, pbr_setup);
        // alpha test 
        clip(pbr_setup.alpha - 1);

        half3 col = 1;
        return half4(col, pbr_setup.alpha);
    }

    // HAIR MAIN FRAG //
    half4 frag_hair (v2f_PBR i) : SV_Target
    {
        // === VARIABLE PREPARATION === //
        PBR_setup pbr_setup;
        SetupPBRVariables(i, pbr_setup);

        PBR_vectors pbr_vectors;
        SetupPBRVectors(i, pbr_vectors);

        // Get direct light info
        half3 SHCol = SampleSH(pbr_vectors.n);
        Light mainLight = GetMainLight(i.shadowCoord);
        half3 Li = SHCol + mainLight.color; 
        

        half3 tangentWS = {i.TBN0.x, i.TBN1.x, i.TBN2.x};
        half3 bitangentWS = {i.TBN0.y, i.TBN1.y, i.TBN2.y};

        // depends on strand directon in uv space
        // along u - tangent
        // along v - bitangent
        half3 t = bitangentWS;

        // === DIRECT LIGHT === //
        // Diffuse // 
        half3 diffCol_DL = pbr_setup.baseCol * lerp(0.25, 1.0, pbr_vectors.nl);
        // Specular // 
        half shiftTex = tex2D(_ShiftTangentTex, i.uv).r - 0.5;
        half3 t1 = ShiftTangent(t, pbr_vectors.n, _Shift.x + shiftTex);
        half3 t2 = ShiftTangent(t, pbr_vectors.n, _Shift.y + shiftTex);

        half3 specCol_DL = Li * StrandSpecular(t1, pbr_vectors.v, pbr_vectors.l, _Glossiness.x)
                            + _SpecCol * StrandSpecular(t2, pbr_vectors.v, pbr_vectors.l, _Glossiness.y);

        // Shadow //
        half shadow = saturate(mainLight.shadowAttenuation + _ShadowIntensity);

        half3 col;
        col = (diffCol_DL + specCol_DL) * Li * shadow;
        half alpha = (pbr_setup.alpha * _Alpha);
        return half4(col, alpha);
    }

#endif