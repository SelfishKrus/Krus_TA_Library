Shader "Environment/VolumeFog"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _FogDensity ("Fog Denisty", Range(0,1)) = 0.028
        _WindDir ("Wind Direction", Vector) = (1,0,1)
        _WindSpeed ("Wind Speed", Range(0, 1)) = 0.1

        _NoiseMap ("Noise Map", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Range(0, 5)) = 1

        _yGradientScale ("Y Gradient Scale", Range(0, 10)) = 0.5
        _xGradientScale ("X Gradient Scale", Range(0, 10)) = 1
        _zGradientScale ("Z Gradient Scale", Range(0, 10)) = 1

        [HideInInspector] _yGradientOffset ("Y Gradient Offset", float) = 0
        [HideInInspector] _xGradientOffset ("X Gradient Offset", float) = 0
        [HideInInspector] _zGradientOffset ("Z Gradient Offset", float) = 0

        _StepSize ("Step Size", Range(0,3)) = 0.1
        _TestFactor0 ("Test Factor0", float) = 1
        _TestFactor1 ("Test Factor1", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            // #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"



            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _MainTex_ST;
            float3 _BoxCenter;
            float3 _BoxExtents;
            float _NoiseScale;

            float _yGradientScale;
            float _xGradientScale;
            float _zGradientScale;

            float _yGradientOffset;
            float _xGradientOffset;
            float _zGradientOffset;

            float _FogDensity;
            float3 _WindDir;
            float _WindSpeed;

            float _StepSize;
            float _TestFactor0;
            float _TestFactor1;
            CBUFFER_END

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseMap);       SAMPLER(sampler_NoiseMap);

            // FUNCTION START /////////////////////////////////////////////////////////////////////

            float2 RayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir)
            {
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                float dstA = max(max(tmin.x, tmin.y), tmin.z); 
                float dstB = min(tmax.x, min(tmax.y, tmax.z)); 
                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }
            
             real3 GetWorldPosByDepth(real2 uv)
             {
                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(uv);
                #else
                    // Adjust z to match NDC for OpenGL
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                #endif
                return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP);
            }

            float getHeightGradient(float heightPercent) 
            {
                float3 HeightUpDownKB = float3(0.5, 0.5, 0.5);
                float heightGradientUp = HeightUpDownKB.x * heightPercent + HeightUpDownKB.y;
                float heightGradientDown = HeightUpDownKB.z * heightPercent;
                float heightGradient = saturate(heightGradientDown) * saturate(heightGradientUp);
                return heightGradient;
            }

            // sample noise map as fog density value
            half SampleDensity(float3 posWS, half3 wind)
            {
                float3 absPosWS = abs(posWS);

                posWS.xyz -= _BoxCenter;
                posWS.y /=  _BoxExtents.y;

                half density = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, posWS.xz * _NoiseScale + wind);
                

                // height gradient 
                float yGradient = _yGradientScale * abs(posWS.y) + _yGradientOffset;
                float xGradient = _xGradientScale * abs(posWS.x) + _xGradientOffset;
                float zGradient = _zGradientScale * abs(posWS.z) + _zGradientOffset;
        
                float gradient = yGradient + xGradient + zGradient;
                density -= gradient ;
                density = saturate(density);

                return density;
            }



            // fog ray marching
            float FogRayMarching(float3 startPos, float3 rayDir, float stepSize, float dstLimit, int maxIteraton, float3 wind)
            {
                float density = 0;
                half dstTraveled = 0;
                float3 currentPos = startPos;
                for (int i = 0; i < maxIteraton; i++)
                {
                    if ( dstLimit > 0 && dstTraveled < dstLimit)
                    {
                        density += _FogDensity * SampleDensity(currentPos, wind);
                        dstTraveled += stepSize;
                        currentPos += rayDir * stepSize ;
                        if (density > 0.95)
                            break;
                    }
                    
                }
                return density;
            }
            

            // FUNCTION END /////////////////////////////////////////////////////////////////////

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                // dist to box & dst inside box
                float3 posWS = i.posWS;
                float3 rayOrigin = _WorldSpaceCameraPos;
                half3 rayDir = normalize(posWS - rayOrigin);
                float2 rayToContainerInfo = RayBoxDst(_BoxCenter - _BoxExtents, _BoxCenter + _BoxExtents, rayOrigin, 1.0 / rayDir);
                float dstToBox = rayToContainerInfo.x;
                float dstInsideBox = rayToContainerInfo.y;

                

                // sample depth texture
                // NDC range [0, 1]^4 in HLSL
                half2 uv = i.vertex.xy / _ScaledScreenParams.xy;
                half depth = SampleSceneDepth(uv);
                
                // calculate world position of pixels from depth texture
                float3 pixelPosWS = GetWorldPosByDepth(uv);
                // distance between obstacle and camera
                float dst_pixelncam = length(pixelPosWS - _WorldSpaceCameraPos);

                // set conditions for rayMarching
                // min() for the situation that obstacles are behind volume fog
                float dstLimit = min(dst_pixelncam - dstToBox, dstInsideBox);

                // wind 
                float3 wind = _WindDir * _Time.y * _WindSpeed;
                wind %= 10000;
                
                //fog ray marching
                float3 startPos = rayOrigin + dstToBox * rayDir;
                int maxIteraton = 32;
                half fogDensity = FogRayMarching(startPos, rayDir, _StepSize, dstLimit, maxIteraton, wind);

                half4 col = 1;
                col.rgba = fogDensity * _Color;
                return col;
            }
            ENDHLSL
        }
    }
}
