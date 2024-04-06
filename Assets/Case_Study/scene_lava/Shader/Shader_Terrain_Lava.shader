Shader "Terrain/Lava"
{
    Properties
    {   
        [Header(VFX Toggle)]
        [Toggle(_UNDER_COLOR_PARALLAX)]_UnderColorParallax ("Under Color Parallax", Float) = 1
        [Toggle(_DOUBLE_LAYER_SURFACE)]_DoubleLayerSurface ("Double Layer Surface", Float) = 1
        _TessellationFactor ("Tessellation Factor", Range(1 ,20)) = 1

        [Space(30)]

        [Header(Rock)]
        _BaseColorTex ("BaseColor Texture", 2D) = "Gray" {}
        _BaseColorTint ("BaseColor Tint", Color) = (1, 1, 1, 1)

        [NoScaleOffset]_NormalTex ("Normal Map", 2D) = "Bump" {}
        _NormalScale ("Normal Scale", Float) = 1

        [NoScaleOffset]_DisplacementTex ("Height Texture", 2D) = "" {}
        _DisplacementScale ("Displacement Scale", Float) = 0
        [Space(30)]

        [Header(Lava)]
        _LavaBaseColorTex_Surface ("Lava Surface BaseColor Tex", 2D) = "Gray" {}
        [NoScaleOffset]_LavaBaseColorTex_Under ("Lava Under BaseColor Tex", 2D) = "Gray" {}
        _LavaHeight ("Lava Height", Float) = 0 
        [HDR]_LavaBloomTint ("Lava Bloom Tint", Color) = (0, 0, 0, 1)
        _LavaBloomSmoothness ("Lava Bloom Smoothness", Float) = 0.01
        _LavaSpeed ("Lava Speed", Vector) = (0.5, 2, 0.5, 1)
        _LavaAmplitude ("Lava Amplitude", Float) = 0.1
        _LavaFrequency ("Lava Frequency", Float) = 1
        _LavaWarpFac ("Lava Warp Factor, x - center, y - total intens", Vector) = (5,4,1,1)

        [Space(20)]
        _LavaParallaxScale ("Lava Parallax Scale", Float) = 30
        _LavaUnderRoughness ("Lava Under Roughness", Range(0, 10)) = 0
        _BlendFac ("Blending Factor", Vector) = (0.25, 5, 0, 0)

        [Space(30)]
        _Test ("Test Factor", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}
        LOD 100
        ZWrite On

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag

            #pragma shader_feature _ _UNDER_COLOR_PARALLAX
            #pragma shader_feature _ _DOUBLE_LAYER_SURFACE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"

            #include "Assets/Common/Shader/K_Pbr.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct tessdata
            {   
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 tbn_x : TEXCOORD1;
                float4 tbn_y : TEXCOORD2;
                float4 tbn_z : TEXCOORD3;
                float3 lavaPosWS : TEXCOOD4;

            };

            float3 UnpackNormalTS(TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), float2 uv, float bumpScale)
            {
                float3 normalTS = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv).xyz * 2.0 - 1.0;
                normalTS.xy *= bumpScale;
                return normalize(normalTS);
            }

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColorTex_ST;
            float3 _BaseColorTint;
            float _NormalScale;
            float _TessellationFactor;
            float _DisplacementScale;

            float3 _LavaBloomTint;
            float _LavaBloomSmoothness;
            float _LavaHeight;
            float3 _LavaSpeed;
            float _LavaAmplitude;
            float _LavaFrequency;
            float2 _BlendFac;

            float _LavaParallaxScale;
            float _LavaUnderRoughness;
            float4 _LavaBaseColorTex_Surface_ST;
            float2 _LavaWarpFac;

            float4 _Test;
            CBUFFER_END

            TEXTURE2D(_BaseColorTex);    SAMPLER(sampler_BaseColorTex);
            TEXTURE2D(_NormalTex);       SAMPLER(sampler_NormalTex);
            TEXTURE2D(_DisplacementTex);       SAMPLER(sampler_DisplacementTex);

            TEXTURE2D(_LavaBaseColorTex_Surface);       SAMPLER(sampler_LavaBaseColorTex_Surface);
            TEXTURE2D(_LavaBaseColorTex_Under);       SAMPLER(sampler_LavaBaseColorTex_Under);

            // VERT SHADER // 
            
            tessdata vert (appdata IN)
            {
                tessdata OUT;
                OUT.pos = (IN.posOS);
                float3 posWS = TransformObjectToWorld(IN.posOS.xyz);

                // tbn 
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(IN.tangentOS);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;
                OUT.tbn_x = float4(tangentWS.x, bitangentWS.x, normalWS.x, posWS.x);
                OUT.tbn_y = float4(tangentWS.y, bitangentWS.y, normalWS.y, posWS.y);
                OUT.tbn_z = float4(tangentWS.z, bitangentWS.z, normalWS.z, posWS.z);

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseColorTex);
                return OUT;
            }

            // HULL SHADER // 

            [domain("tri")]     // work with triangle
            [outputcontrolpoints(3)]    // output 3 control points per patch
            [outputtopology("triangle_cw")]     // output triangle with clockwise winding
            [partitioning("integer")]   // integer partitioning mode
            [patchconstantfunc("ConstantFunction")]    // use constant function to calculate patch constant data
            tessdata hull(InputPatch<tessdata, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            // DOMAIN SHADER // 

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            TessellationFactors ConstantFunction(InputPatch<tessdata, 3> patch)
            {
                TessellationFactors f;
                f.edge[0] = _TessellationFactor.x;
                f.edge[1] = _TessellationFactor.x;
                f.edge[2] = _TessellationFactor.x;
                f.inside = _TessellationFactor.x;
                return f;
            }

            [domain("tri")]
            tessdata domain(TessellationFactors factors, OutputPatch<tessdata, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                tessdata data;

                // Interpolation
                #define DOMAIN_INTERPOLATE(fieldName) data.fieldName = \
                    patch[0].fieldName * barycentricCoordinates.x + \
                    patch[1].fieldName * barycentricCoordinates.y + \
                    patch[2].fieldName * barycentricCoordinates.z;
                
                DOMAIN_INTERPOLATE(uv)
                DOMAIN_INTERPOLATE(tbn_x)
                DOMAIN_INTERPOLATE(tbn_y)
                DOMAIN_INTERPOLATE(tbn_z)

                // DISPLACEMENT // 
                //// rock 
                float3 posWS = float3(data.tbn_x.w, data.tbn_y.w, data.tbn_z.w);
                float3 normalWS = float3(data.tbn_x.z, data.tbn_y.z, data.tbn_z.z);
                float displacement = SAMPLE_TEXTURE2D_LOD(_DisplacementTex, sampler_DisplacementTex, data.uv, _BlendFac.y);
                posWS += normalWS * displacement * _DisplacementScale;

                //// lava 
                float3 posWS_lava = posWS;
                posWS_lava.y = _LavaHeight;
                posWS_lava.y += sin(_Time.y * _LavaSpeed.y + (posWS_lava.x + posWS_lava.z)*_LavaFrequency) * _LavaAmplitude * 0.1;
                data.lavaPosWS = posWS_lava;

                // LERP // 
                float vLerpFac = smoothstep(posWS_lava.y, posWS_lava.y+_BlendFac.x*0.1, posWS.y);
                posWS = lerp(posWS_lava, posWS, vLerpFac);
                data.pos = TransformWorldToHClip(posWS);
                 
                return data;
            }

            // FRAG SHADER // 

            half4 frag (tessdata IN) : SV_Target
            {   
                // PRE // 
                float3x3 m_TS2WS = float3x3(IN.tbn_x.xyz, IN.tbn_y.xyz, IN.tbn_z.xyz);
                float3x3 m_WS2TS = transpose(m_TS2WS);
                float3 posWS = float3(IN.tbn_x.w, IN.tbn_y.w, IN.tbn_z.w);
                float3 viewDirWS = normalize(posWS - _WorldSpaceCameraPos);
                float3 viewDirTS = mul(m_WS2TS, viewDirWS);

                //// normal 
                float3 normalTS = UnpackNormalTS(TEXTURE2D_ARGS(_NormalTex, sampler_NormalTex), IN.uv, _NormalScale);
                float3 normalWS_map = mul(m_TS2WS, normalTS);
                float3 normalWS_geo = float3(IN.tbn_x.z, IN.tbn_y.z, IN.tbn_z.z);

                //// directional light
                Light mainLight = GetMainLight();
                float NoL01 = dot(normalWS_map, mainLight.direction) * 0.5 + 0.5;
                float NoL = max(dot(normalWS_map, mainLight.direction), 0.00001);

                //// displacement 
                float displacement = SAMPLE_TEXTURE2D_LOD(_DisplacementTex, sampler_DisplacementTex, IN.uv, _BlendFac.y);
                float displacement_lod5 = SAMPLE_TEXTURE2D_LOD(_DisplacementTex, sampler_DisplacementTex, IN.uv, 5);

                // ROCK // 
                //// diffuse 
                half3 baseColor = SAMPLE_TEXTURE2D(_BaseColorTex, sampler_BaseColorTex, IN.uv).rgb * _BaseColorTint;
                half3 diffuse = baseColor * NoL * mainLight.color;

                //// lava bloom
                float posY = posWS.y+displacement;
                half3 lavaBloom = (1-smoothstep(IN.lavaPosWS.y, IN.lavaPosWS.y+_LavaBloomSmoothness, posY)) * _LavaBloomTint;
                lavaBloom *= baseColor;

                half3 rockCol = diffuse + lavaBloom + unity_AmbientSky.rgb;

                // LAVA // 
                // diffuse 
                // surface
                float2 uv_surface = TRANSFORM_TEX(IN.uv, _LavaBaseColorTex_Surface);
                float speedNoise = (1 - (displacement_lod5 + _LavaWarpFac.x) / (1 + _LavaWarpFac.x)) * _LavaWarpFac.y;
                float time = _Time.y;
                #ifdef _DOUBLE_LAYER_SURFACE
                    float2 uv_surface0 = uv_surface + _LavaSpeed.xz * time * 0.1 + speedNoise;
                    float2 uv_surface1 = uv_surface + float2(0.5, 0.5) + _LavaSpeed.xz * time * 0.05;
                    half3 surfaceCol0 = SAMPLE_TEXTURE2D(_LavaBaseColorTex_Surface, sampler_LavaBaseColorTex_Surface, uv_surface0).rgb;
                    half3 surfaceCol1 = SAMPLE_TEXTURE2D(_LavaBaseColorTex_Surface, sampler_LavaBaseColorTex_Surface, uv_surface1).rgb;
                    half3 surfaceCol = (surfaceCol0 * 0.6 + surfaceCol1 * 0.3);
                #else 
                    uv_surface += _LavaSpeed.xz * time * 0.1 + speedNoise;
                    half3 surfaceCol = SAMPLE_TEXTURE2D(_LavaBaseColorTex_Surface, sampler_LavaBaseColorTex_Surface, uv_surface).rgb;
                #endif 

                // under
                // parallax
                #ifdef _UNDER_COLOR_PARALLAX
                    float2 uv_under = IN.uv + ParallaxMapping(_DisplacementTex, sampler_DisplacementTex, -viewDirTS, _LavaParallaxScale * 0.01, IN.uv);
                #else 
                    float2 uv_under = IN.uv;
                #endif
                half3 underCol = SAMPLE_TEXTURE2D_LOD(_LavaBaseColorTex_Under, sampler_LavaBaseColorTex_Under, uv_under, _LavaUnderRoughness);

                half3 lavaCol = surfaceCol + underCol;

                half3 col = 1.0;
                float lerpFac = smoothstep(IN.lavaPosWS.y, IN.lavaPosWS.y+_BlendFac.x*0.1, posWS.y+displacement);
                col = lerp(lavaCol, rockCol, lerpFac);
                return half4(col, 1); 
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
