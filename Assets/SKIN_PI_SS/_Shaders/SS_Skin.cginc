#ifndef ___SS_SKIN___
#define ___SS_SKIN___

#include "SS_Utils.cginc"
#include "SS_Common.cginc"
#include "SS_Lighting.cginc"

v2f vert(appdata v)
{
    v2f o;

    o.uv = v.uv;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
    TRANSFER_SHADOW(o);

    return o;
}

float4 frag(v2f i) : SV_Target
{
    float3 albedo = tex2D(_MainTex, i.uv).rgb;
    float specReflectance = tex2D(_SpecularTex, i.uv);
    float3 datas = tex2D(_DatasTex, i.uv);

    float3 ambientLight = 0;
				
    UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
    
    float3 lightDir = _WorldSpaceLightPos0.xyz;
	
    #if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
        lightDir = normalize(lightDir - i.worldPos);
    #endif

    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    i.normal = normalize(i.normal);
    i.tangent.xyz = normalize(i.tangent.xyz);
    float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);

    float3x3 matTangentToWorld = float3x3(i.tangent.xyz, binormal, i.normal);

    float3 normalTangent = UnpackSNormal(tex2D(_NormalTex, i.uv), _BumpScale);
    float3 normalTangentBlurred = UnpackSNormal(tex2Dbias(_NormalTex, float4(i.uv, 0, _NormalBlurredBias)), _BumpScale);

    float3 normalShade = normalize(mul(normalTangent, matTangentToWorld));
    float3 normalBlurred = normalize(mul(normalTangentBlurred, matTangentToWorld));
				
    float3 rgbLitDiffuse = EvaluateDiffuseLight(lightDir, albedo, i.normal, normalShade, normalBlurred, datas.r, attenuation, ambientLight) * datas.g;

    float3 rgbLitSpecular = EvaluateSpecularLight(lightDir, i.uv, i.normal, normalShade, viewDir, specReflectance, _Glossiness, attenuation) * datas.g;
    rgbLitSpecular *= _SpecularIntensity;
    
    float3 translucency = EvaluateFastTranslucency(lightDir, datas.b, albedo, i.normal, normalShade, viewDir);

    float3 col = rgbLitDiffuse + rgbLitSpecular + translucency;
        		
    return Tonemap(col);
}

#endif