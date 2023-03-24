Shader "Common/UVFog"
{
    Properties {
		_FogDensity ("Fog Density", Float) = 0.55
		_FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
		_FogStart ("Fog Start", Float) = 0.0
		_FogEnd ("Fog End", Float) = 21.5
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_FogXSpeed ("Fog Horizontal Speed", Float) = 0.005
		_FogYSpeed ("Fog Vertical Speed", Float) = 0
		_NoiseAmount ("Noise Amount", Float) = 0.5
	}
    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline" "Queue"="Transparent"}
		
		
		Pass {
			Tags {"LightMode"="UniversalForward"}
			
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
            half _FogDensity;
            half4 _FogColor;
            float _FogStart;
            float _FogEnd;
            half _FogXSpeed;
            half _FogYSpeed;
            half _NoiseAmount;
            float4x4 _FrustumCornersRay;
        	CBUFFER_END
			
            TEXTURE2D(_CameraDepthTexture);                SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_NoiseTex);                          SAMPLER(sampler_NoiseTex);
		
        	struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
        	};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 interpolatedRay : TEXCOORD1;
			};
		
			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = TransformObjectToHClip(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				o.uv.zw = v.texcoord;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_NoiseTex_TexelSize.y < 0)
					o.uv.w = 1 - o.uv.w;
				#endif
				
				int index = 0;
				if (v.texcoord.x < 0.5 && v.texcoord.y < 0.5) {
					index = 0;
				} else if (v.texcoord.x > 0.5 && v.texcoord.y < 0.5) {
					index = 1;
				} else if (v.texcoord.x > 0.5 && v.texcoord.y > 0.5) {
					index = 2;
				} else {
					index = 3;
				}
				#if UNITY_UV_STARTS_AT_TOP
				if (_NoiseTex_TexelSize.y < 0)
					index = 3 - index;
				#endif
				
				o.interpolatedRay = _FrustumCornersRay[index];
						
				return o;
			}
		
			half4 frag(v2f i) : SV_Target 
			{	
				// the distance between frag and cam
				float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.zw), _ZBufferParams);
				// world position of frag
				float3 worldPos = _WorldSpaceCameraPos + linearDepth * i.interpolatedRay.xyz;
				
				float2 speed = _Time.y * float2(_FogXSpeed, _FogYSpeed);
				float noise = (SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv.xy + speed).r);
				noise = pow(noise, 1 / _NoiseAmount);

				float fogDensity = (_FogEnd - worldPos.y) / (_FogEnd - _FogStart);
				fogDensity = saturate(fogDensity * _FogDensity * (1 + noise));
				
				half4 col = fogDensity * _FogColor;
				// col.rgb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
				// col.rgb = lerp(col.rgb, _FogColor.rgb, fogDensity);
				// col.a = noise;
				
				return col;
			}
		
			ENDHLSL
		}
	} 
	FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}
