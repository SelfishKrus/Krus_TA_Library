Shader "FI/Skin"
{
    Properties
	{
		// Tone Mapping
		_Exposure("Exposure", Range(0, 2)) = 1
		[Toggle] _Tonemap("Use Tone Mapping", Float) = 1

		// Albedo
		_Color("Color", Color) = (1, 1, 1, 1)
		[NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}

		// Specular Reflectance
		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		_SpecularIntensity("Specular Intensity", Range(0, 2)) = 1
		[NoScaleOffset] _SpecularTex("Specular (R)", 2D) = "white" {}
		[NoScaleOffset] _SpecularMaskTex("Specular Mask (R)", 2D) = "white" {}

		// Curvature (R), Ambient Occlussion (G), and Thickness (B)
		_DatasTex("Cu (R), AO (G), Th (B)", 2D) = "white" {}

		// Fast Translucency
		[HDR] _TrnColor("Translucency Color", Color) = (1, 0, 0, 0)
		_TrnDistortion("Distortion", Range(0, 1)) = 0.5
		_TrnPower("Power", Range(1, 12)) = 12
			
		// Bump
		_BumpScale("BumpScale", Range(0, 2)) = 1
		_NormalBlurredBias("NormalBlurredBias", Range(0, 10)) = 3
		[NoScaleOffset] _NormalTex("NormalTexture", 2D) = "bump" {}
			
		// Curvature LUT
		[NoScaleOffset] _CurvatureTex_LUT("Curvature LUT", 2D) = "black" {}
		_CurvatureBias("CurvatureBias", Vector) = (1, 0, 0, 0)

		// Shadow LUT
		[NoScaleOffset] _ShadowTex_LUT("Shadow LUT", 2D) = "black" {}
		_ShadowBias("ShadowBias", Vector) = (1, 0, 0, 0)

		// Diffuse IBL
		[NoScaleOffset] _DiffuseEnvironment("Diffuse Environment (IBL)", CUBE) = "" {}
		_DiffuseEnvironmetAmount("Diffuse Environment Amount", Range(0, 1)) = 1

		// Specular IBL
		[NoScaleOffset] _SpecularEnvironment("Specular Environment (IBL)", CUBE) = "" {}
		_SpecularEnvironmetAmount("Specular Environment Amount", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags { "LightMode" = "UniversalForward" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

			#pragma multi_compile_fwdbase
			#pragma multi_compile _ _TONEMAP_ON

			#define FORWARD_BASE_PASS

			#include "SS_Skin.cginc"
				
            ENDCG
        }

		Pass
		{
			Tags { "LightMode" = "ForwardAdd" }

			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fwdadd
			#pragma multi_compile _ _TONEMAP_ON

			#include "SS_Skin.cginc"

			ENDCG
		}

		Pass
		{
			Tags { "LightMode"="ShadowCaster" }

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster
	
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			#if defined(SHADOWS_CUBE)

			struct v2f {
				float4 pos : SV_POSITION;
				float3 lightVec : TEXCOORD0;
			};

			float4 vert(appdata v) : SV_POSITION
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.lightVec = mul(unity_ObjectToWorld, v.vertex).xyz - _LightPositionRange.xyz;
				return o;
			}

			float4 frag() : SV_Target
			{
				return 0;
			}

			#else

			float4 vert(appdata v) : SV_POSITION 
			{
				float4 position = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);
				return UnityApplyLinearShadowBias(position);
			}

			float4 frag() : SV_Target
			{
				return 0;
			}

			#endif

			ENDCG
		}
    }
}
