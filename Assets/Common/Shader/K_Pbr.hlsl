#ifndef K_PBR
#define K_PBR

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

	float3 Fresnel_SchlickRoughness(float cosTheta, float3 F0, float roughness)
	{
		return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
	}

	

	half3 GetDiffuse_HalfLambert(Light mainLight, float3 normal)
	{
		
	}

#endif 