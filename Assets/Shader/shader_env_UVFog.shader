Shader "Environment/UVFog"
{
    Properties {
		_NoiseAmount ("Fog Density", Range(0, 5)) = 1
		_FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_FogXSpeed ("Fog Horizontal Speed", Float) = 0.005
		_FogYSpeed ("Fog Vertical Speed", Float) = 0
		_MaskHeight	("Mask Height", Range(0, 5)) = 0.5

		[Header(Test Factors)]
		_TestFactor0 ("Test Factor 0", Float) = 0
		_TestFactor1 ("Test Factor 1", Float) = 0
	}
    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline" "Queue"="Transparent"}
		
		
		Pass {
			Tags {"LightMode"="UniversalForward"}
			
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front

			HLSLPROGRAM  
			
			#pragma vertex vert  
			#pragma fragment frag 
		
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        	CBUFFER_START(UnityPerMaterial)
			half4 _NoiseTex_ST;
			half4 _NoiseTex_TexelSize;
            half4 _FogColor;
            half _FogXSpeed;
            half _FogYSpeed;
            half _NoiseAmount;
			half _MaskHeight;
			float _TestFactor0;
			float _TestFactor1;
        	CBUFFER_END
			
            TEXTURE2D(_CameraDepthTexture);                SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_NoiseTex);                          SAMPLER(sampler_NoiseTex);
		
        	struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
        	};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 posWS : TEXCOORD1;
			};
		
			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				o.posWS = TransformObjectToWorld(v.vertex.xyz).xyz;
				o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
				return o;
			}
		
			half4 frag(v2f i) : SV_Target 
			{	
				
				// fog
				float2 speed = _Time.y * float2(_FogXSpeed, _FogYSpeed);
				float noise = saturate(SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv + speed).r);
				noise = pow(noise, 1 / _NoiseAmount);
				
				// mask
				half3 up = half3(0, 1, 0);
				half3 dir = normalize(i.posWS - _WorldSpaceCameraPos);
				half mask = saturate(dot(up, dir));
				mask = 1 - smoothstep(0, _MaskHeight, mask);

				half alpha = mask * noise;
				half3 col = _FogColor.rgb;
				
				return float4(col, alpha);
			}
		
			ENDHLSL
		}
	} 
	FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}
