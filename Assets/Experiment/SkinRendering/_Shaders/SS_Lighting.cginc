#ifndef ___SS_LIGHTING___
#define ___SS_LIGHTING___

#include "SS_Common.cginc"

float3 EvaluateFastTranslucency(float3 lightDir, float thickness, float3 albedo, float3 normalGeom, float3 normalShade, float3 viewDir)
{
    float3 H = normalize(lightDir + normalGeom * _TrnDistortion);
    float VoH = pow(saturate(dot(viewDir, -H)), _TrnPower);
    float3 I = _TrnColor * albedo * VoH * thickness;

    return I;
}

float3 EvaluateDiffuseLight(float3 lightDir, float3 albedo, float3 normalGeom, float3 normalShade, float3 normalBlurred, float curvature, float shadow, out float3 ambienLight)
{
    ambienLight = 0;

    // diffuse lighting based on curvature
    float NoL_BlurredUnclamped = dot(normalBlurred, lightDir);
    float curvatureScaled = curvature * _CurvatureBias.x + _CurvatureBias.y;
    float2 uvCurvature = float2(NoL_BlurredUnclamped * 0.5 + 0.5, curvatureScaled);
    float3 rgbCurvature = tex2D(_CurvatureTex_LUT, uvCurvature) * 0.5 - 0.25; // [0, 1] => [-0.25, 0.25]

    // N dot L for different color channels
    // lambert??
    float normalSmoothFactor = saturate(1.0 - NoL_BlurredUnclamped);
    normalSmoothFactor *= normalSmoothFactor;
    float3 normalShadeG = normalize(lerp(normalShade, normalBlurred, 0.3 + 0.7 * normalSmoothFactor));
    float3 normalShadeB = normalize(lerp(normalShade, normalBlurred, normalSmoothFactor));
    float NoL_ShadeG = saturate(dot(normalShadeG, lightDir));
    float NoL_ShadeB = saturate(dot(normalShadeB, lightDir));
    float3 rgbNoL = float3(saturate(NoL_BlurredUnclamped), NoL_ShadeG, NoL_ShadeB);

    float3 rgbSSS = saturate(rgbCurvature + rgbNoL);

    float NoL = saturate(dot(normalGeom, lightDir));
    float2 uvShadow = { shadow, NoL * _ShadowBias.x + _ShadowBias.y };

    float3 rgbShadow = tex2D(_ShadowTex_LUT, uvShadow);

    float3 rgbLightDiffuse = _LightColor0.rgb * rgbSSS * rgbShadow;
        
    #ifdef FORWARD_BASE_PASS

	    // IBL Diffuse
        float3 normalAmbient0 = normalBlurred;
        float3 normalAmbient1 = normalize(lerp(normalShade, normalBlurred, 0.3));
        float3 normalAmbient2 = normalShade;

        float3 IBL_Diffuse = float3(
		    texCUBE(_DiffuseEnvironment, normalAmbient0).r,
		    texCUBE(_DiffuseEnvironment, normalAmbient1).g,
		    texCUBE(_DiffuseEnvironment, normalAmbient2).b);

        ambienLight = IBL_Diffuse;

        rgbLightDiffuse += IBL_Diffuse * _DiffuseEnvironmetAmount;

    #endif

    float3 rgbLitDiffuse = albedo * rgbLightDiffuse;
 
    return rgbLitDiffuse;
}

float3 EvaluateSpecularIBL(float gloss, float3 viewDir, float3 normalShade, float specReflectance, float specLobeBlend, float NoV)
{
    float3 vecReflect = reflect(-viewDir, normalShade);
    float gloss0 = gloss;
    float gloss1 = saturate(2.0 * gloss);
    float fresnelIBL0 = lerp(specReflectance, 1.0, pow(1.0 - NoV, 5.0) / (-3.0 * gloss0 + 4.0));
    float mipLevel0 = -9.0 * gloss0 + 9.0;
    float3 iblSpec0 = fresnelIBL0 * texCUBElod(_SpecularEnvironment, float4(vecReflect, mipLevel0));
    float fresnelIBL1 = lerp(specReflectance, 1.0, pow(1.0 - NoV, 5.0) / (-3.0 * gloss1 + 4.0));
    float mipLevel1 = -9.0 * gloss1 + 9.0;
    float3 iblSpec1 = fresnelIBL1 * texCUBElod(_SpecularEnvironment, float4(vecReflect, mipLevel1));

    return lerp(iblSpec0, iblSpec1, specLobeBlend) * _SpecularEnvironmentAmount;
}

float3 EvaluateSpecularLight(float3 lightDir, float2 uv, float3 normalGeom, float3 normalShade, float3 viewDir, float specReflectance, float gloss, float shadow)
{				
    float3 lightColor = _LightColor0.rgb;

    float3 halfVec = normalize(lightDir + viewDir);
    float NoL = saturate(dot(normalShade, lightDir));
    float NoH = saturate(dot(normalShade, halfVec));
    float LoH = dot(lightDir, halfVec);
    float NoV = saturate(dot(normalShade, viewDir));
    float specPower = exp2(gloss * 13.0);

	// Evaluate NDF and visibility function
	// Two-lobe Blinn-Phong, with double gloss on second lobe
    float specLobeBlend = 0.05;
    float specPower0 = specPower;
    float specPower1 = specPower * specPower;
				
    float ndf0 = pow(NoH, specPower0) * (specPower0 + 2.0) * 0.5;
    float schlickSmithFactor0 = rsqrt(specPower0 * (UNITY_PI * 0.25) + (UNITY_PI * 0.5));
    float visibilityFn0 = 0.25 / (lerp(schlickSmithFactor0, 1, NoL) * lerp(schlickSmithFactor0, 1, NoV));

    float ndf1 = pow(NoH, specPower1) * (specPower1 + 2.0) * 0.5;
    float schlickSmithFactor1 = rsqrt(specPower1 * (UNITY_PI * 0.25) + (UNITY_PI * 0.5));
    float visibilityFn1 = 0.25 / (lerp(schlickSmithFactor1, 1, NoL) * lerp(schlickSmithFactor1, 1, NoV));
    float ndfResult = lerp(ndf0 * visibilityFn0, ndf1 * visibilityFn1, specLobeBlend);

    float fresnel = lerp(specReflectance, 1.0, pow(1.0 - LoH, 5.0));
    float specResult = ndfResult * fresnel;
    
    float edgeDarken = saturate(5.0 * dot(normalGeom, lightDir));
    float3 rgbLitSpecular = lightColor * NoL * edgeDarken * specResult * shadow;

	// IBL Spec
    float3 IBL_Specular = EvaluateSpecularIBL(gloss, viewDir, normalShade, specReflectance, specLobeBlend, NoV);

    rgbLitSpecular += IBL_Specular;

    // Specular Mask
    float specularMask = tex2D(_SpecularMaskTex, uv).r;

    return rgbLitSpecular * specularMask;
}

#endif