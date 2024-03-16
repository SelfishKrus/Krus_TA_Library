#ifndef K_PBR
#define K_PBR

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

	float3 UnpackNormalTS(TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), float2 uv, float bumpScale)
	{
		float3 normalTS = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv).xyz * 2.0 - 1.0;
		normalTS.xy *= bumpScale;
		return normalize(normalTS);
	}

	half3 GetDiffuse_HalfLambert(Light mainLight, float3 normal)
	{
		
	}

#endif 