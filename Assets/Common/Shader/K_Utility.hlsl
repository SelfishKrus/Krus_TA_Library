#ifndef K_UTILITY
#define K_UTILITY
    
    // TBN 
    // | T.x  B.x  N.x posWS.x |
    // | T.y  B.y  N.y posWS.y |
    // | T.z  B.z  N.z posWS.z |

    struct TbnData
    {
        float3x3 m_TS2WS;
        float3x3 m_WS2TS;
        float3 posWS;
        float3 normalWS;
    };

    void EncodeTbn(in appdata IN, inout v2f OUT)
	{
        // tbn 
        float3 posWS = TransformObjectToWorld(IN.posOS);
        float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
        float3 tangentWS = TransformObjectToWorldDir(IN.tangentOS);
        float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;
        OUT.tbn_r0 = float4(tangentWS.x, bitangentWS.x, normalWS.x, posWS.x);
        OUT.tbn_r1 = float4(tangentWS.y, bitangentWS.y, normalWS.y, posWS.y);
        OUT.tbn_r2 = float4(tangentWS.z, bitangentWS.z, normalWS.z, posWS.z);
    }

    void DecodeTbn(in v2f IN , inout TbnData tbnData)
    {   
        tbnData.m_TS2WS = float3x3(IN.tbn_r0.xyz, IN.tbn_r1.xyz, IN.tbn_r2.xyz);
        tbnData.m_WS2TS = transpose(tbnData.m_TS2WS);
        tbnData.posWS = float3(IN.tbn_r0.w, IN.tbn_r1.w, IN.tbn_r2.w);
        tbnData.normalWS = float3(IN.tbn_r0.z, IN.tbn_r1.z, IN.tbn_r2.z);
    }

    // NORMAL // 
    float3 UnpackNormalTS(TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), float2 uv, float bumpScale)
	{
		float3 normalTS = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv).xyz * 2.0 - 1.0;
		normalTS.xy *= bumpScale;
		return normalize(normalTS);
	}

    // DESATURATE // 
    float Desaturate(float3 color)
    {
        return dot(color, float3(0.299, 0.587, 0.114));
    }

#endif