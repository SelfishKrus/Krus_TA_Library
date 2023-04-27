#ifndef ___SS_VARIABLES_INCLUDED____
#define ___SS_VARIABLES_INCLUDED____

#include "SS_Utils.cginc"
#include "AutoLight.cginc"
#include "ToneMapping.cginc"
#include "UnityStandardBRDF.cginc"

float4 _TrnColor, _Color;

float2 _CurvatureBias, _ShadowBias;

float _IrisRadiusSource, _IrisRadiusDest, _IrisEdgeHardness, _IrisDilation;

float _AlphaMultiplier, _Glossiness, _Anisotropy, _SpecularIntensity, _TrnAmbient, _BumpScale, _NormalBlurredBias, _DiffuseEnvironmetAmount, _SpecularEnvironmentAmount, _TrnDistortion, _TrnPower;

samplerCUBE _DiffuseEnvironment, _SpecularEnvironment;

sampler2D _MainTex, _IrisTex, _SpecularTex, _DatasTex, _SpecularMaskTex, _NormalTex, _CurvatureTex_LUT, _ShadowTex_LUT;

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    float3 normal : TEXCOORD2;
    float4 tangent : TEXCOORD3;
    float4 pos : SV_POSITION;
    SHADOW_COORDS(4)
};

#endif