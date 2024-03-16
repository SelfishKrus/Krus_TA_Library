Shader "Environment/GeometryGrass"
{
    Properties
    {   
        [Header(Color)]
        _TopCol ("Top Color", Color) = (0,1,0,1)
        _BotCol ("Bottom Color", Color) = (0,0.5,0,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        [Space(10)]

        [Header(Shadow)]
        [Toggle(_MAIN_LIGHT_SHADOWS)] _MAIN_LIGHT_SHADOWS("Receive Shadow", Float) = 1
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.25
        [Header(Fake Shadow)]
        _FakeShadowIntensity ("Fake Shadow Intensity", Range(0, 1)) = 0.25
        _FakeShadowRange ("Fake Shadow Range", Range(0.1, 1)) = 0.5


        [Header(Shape Control)]
        _BendAmount ("Bend Amount", Range(0, 1)) = 0
        _BladeWidth ("Blade Width", Float) = 0.5
        _BladeHeight ("Blade Height", Float) = 1  
        _BladeWidthRand ("Blade Width Randomness", Range(0, 1)) = 0.1
        _BladeHeightRand ("Blade Height Randomness", Range(0, 1)) = 0
        [IntRange] _BladeBendCurve ("Blade Bend Curve", Range(1, 4)) = 2
        _BladeBendDistance ("Blade Bend Distance", Range(0, 3)) = 1.0
        _TessellationnEdgeLength ("Grass Inv_Density (Tessellation Edge Length)", Range(0, 0.1)) = 0.05
        [IntRange] _MaxEdgeFactor ("Max density (Max Edge Factor)", Range(1, 10)) = 4
        [Space(30)]

        [Header(Interactor)]
        _InteractorRadius ("Interactor Radius", Float) = 0.5
        _InteractorStrength ("Interactor Strength", Float) = 5
        [Space(30)]

        [Header(Wind)]
        _WindNoiseMap ("Wind Noise Map", 2D) = "white" {}
        _WindNoiseFrequency ("Wind Noise Frequency", Vector) = (0.05, 0.05, 0, 0)
        _WindStrength ("Wind Strength", Float) = 0.1
        [Space(30)]

        _CullingThreshold ("Culling Threshold", Float) = 10


        _Test ("Test Factor", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" }
       

        HLSLINCLUDE
            #pragma target 4.6

            #define BLADE_SEG_NUM 3

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Common/Shader/KrusNoise.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            CBUFFER_START(UnityPerMaterial)
            half4 _TopCol;
            half4 _BotCol;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            half _BendAmount;
            half _BladeWidth;
            half _BladeHeight;
            half _BladeWidthRand;
            half _BladeHeightRand;
            int _BladeBendCurve;
            float _BladeBendDistance;
            float _TessellationnEdgeLength;
            half _MaxEdgeFactor;
            
            sampler2D _WindNoiseMap;
            float4 _WindNoiseMap_ST;
            float2 _WindNoiseFrequency;
            float _WindStrength;

            half _ShadowIntensity;
            half _FakeShadowIntensity;
            half _FakeShadowRange;
            
            uniform float3 _InteractorPos0;
            half _InteractorRadius;
            float _InteractorStrength;

            float _CullingThreshold;

            float4 _Test;
            CBUFFER_END

            // TEXTURE2D(_WindNoiseMap);   SAMPLER(sampler_WindNoiseMap);

            struct appdata
            {
                float4 pos : POSITION;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half2 uv : TEXCOORD0;
                half3 color : COLOR0;
            };

            struct geomdata
            {
                float4 pos : SV_POSITION;
                half3 normal : NORMAL;
                // half4 tangent : TANGENT;
                half2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                half3 color : COLOR0;
            };

            // FUNCTION START ////////////////////////////////////////////////////////

            // assign properties to vertices created in geom shader
            geomdata GenerateGrassVertices(
                float3 newPosOS, 
                float3 offset, 
                float3x3 matrix_transformation, 
                half2 uv,
                half3 color,
                half3 faceNormalWS)
            {
                geomdata o;
                o.pos = TransformObjectToHClip(newPosOS + mul(matrix_transformation, offset));
                o.uv = uv;
                o.posWS = TransformObjectToWorld(newPosOS + mul(matrix_transformation, offset)).xyz;
                o.color = color;
                o.normal = faceNormalWS;
                return o; 
            }

            // Construct a rotation matrix that rotates around the provided axis
            float3x3 angleAxis3x3(float angle, float3 axis)
			{
				float c, s;
				sincos(angle, s, c);

				float t = 1 - c;
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;

				return float3x3
				(
					t * x * x + c, t * x * y - s * z, t * x * z + s * y,
					t * x * y + s * z, t * y * y + c, t * y * z - s * x,
					t * x * z - s * y, t * y * z + s * x, t * z * z + c
				);
			}

            // FUNCTION END ////////////////////////////////////////////////////////

            // VERT SHADER //
            appdata vert (appdata v)
            {
                appdata o;
                o.pos = v.pos;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.uv = v.uv;
                o.color = v.color;

                

                return o;
            }

            // GEOM SHADER //
            [maxvertexcount(BLADE_SEG_NUM * 2 + 1)]
            void geom (triangle appdata IN[3] : SV_POSITION, inout TriangleStream<geomdata> triStream)
            {
                // prepare variables
                float3 posOS = IN[0].pos.xyz ;
                float3 posWS = TransformObjectToWorld(posOS).xyz;
                half3 normal = IN[0].normal;
                half3 tangent = IN[0].tangent.xyz;
                half3 bitangent = cross(normal, tangent) * IN[0].tangent.w;

                // wind
                float2 windUV = posOS.xz * _WindNoiseMap_ST.xy + _WindNoiseMap_ST.zw + _WindNoiseFrequency * _Time.y;
                half2 windSample = (tex2Dlod(_WindNoiseMap, float4(windUV,0,0)).xy * 2 - 1) * _WindStrength;
                // rotation axis
                half3 windAxis = normalize(float3(windSample.x, windSample.y, 0));
                float3x3 matrix_windRotation = angleAxis3x3(PI * windSample.y, windAxis) ;

                // TBN matrix - TS to OS
                float3x3 matrix_TS2OS = transpose(float3x3(tangent, bitangent, normal));
                float3x3 matrix_randFaceRotation = angleAxis3x3(rand(posOS) * PI * 2, float3(0, 0, 1));
                float3x3 matrix_randBendRotation = angleAxis3x3(rand(posOS.zyx) * _BendAmount * PI * 0.5, float3(1, 0, 0));

                // final rotation matrix for base or top
                float3x3 matrix_transformation_tip = mul(mul(mul(matrix_TS2OS, matrix_windRotation), matrix_randFaceRotation), matrix_randBendRotation);
                float3x3 matrix_transformation_base = mul(matrix_TS2OS, matrix_randFaceRotation);

                // grass height & width
                half width = (rand(posOS.xyz) * 2 - 1) * _BladeWidthRand + _BladeWidth;
                half height = (rand(posOS.zyx) * 2 - 1) * _BladeHeightRand + _BladeHeight;
                half forward = rand(posOS.yxz) * _BladeBendDistance;

                // set grass size from Unity Editor
                width *= IN[0].uv.x;
                height *= IN[0].uv.y;

                // Interactivity 
                float dst = distance(_InteractorPos0, posWS);
                float3 radius = 1 - saturate (dst / _InteractorRadius);
                float3 sphereDisp = posWS - _InteractorPos0;
                sphereDisp *= radius;
                sphereDisp = clamp(sphereDisp * _InteractorStrength, -0.8, 0.8);

                // Face normal
                half3 faceNormalTS = half3(0,1,0);

                // GENERATE VERTICES //
                // segment part
                for (int i = 0; i < BLADE_SEG_NUM; i++)
                {   
                    
                    // dwindling factor
                    float t = i / (float)BLADE_SEG_NUM;
                    // added vertices' pos info
                    // offset done in Tangent space so offset.z is height
                    float3 offset = float3(width * (1 - t),   pow(t, _BladeBendCurve) * forward , height * t);
                    float3x3 matrix_transformation = (i == 0) ? matrix_transformation_base : matrix_transformation_tip;

                    half3 faceNormalWS = TransformObjectToWorldDir(mul(matrix_transformation, faceNormalTS));

                    // interactor force field
                    float3 newPosOS = (i == 0) ? posOS : posOS + sphereDisp * t;
                
                    triStream.Append(GenerateGrassVertices(newPosOS, float3(offset.x, offset.y, offset.z), matrix_transformation, float2(0, t), IN[0].color, faceNormalWS));
                    triStream.Append(GenerateGrassVertices(newPosOS, float3(-offset.x, offset.y, offset.z), matrix_transformation, float2(1, t), IN[0].color, faceNormalWS));
                }
                
                // tip part
                half3 faceNormalWS = TransformObjectToWorldDir(mul(faceNormalTS, matrix_transformation_tip));
                triStream.Append(GenerateGrassVertices(posOS + sphereDisp * 1.3, float3(0, forward, height), matrix_transformation_tip, float2(0.5, 1), IN[0].color, faceNormalWS));
                
                triStream.RestartStrip();
            }

        ENDHLSL

        Pass
        {
            Name "GrassLighting"
            ZWrite On
            ZTest LEqual

            Tags 
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma require geometry
            #pragma require tessellation tessHW

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            
            // FRAG SHADER //
            half4 frag (geomdata i) : SV_Target
            {   
                // culling based on distance
                float dst = distance(i.posWS, _WorldSpaceCameraPos);
                clip(_CullingThreshold - dst);

                // grass color
                half3 col = 1;
                half3 texCol = tex2D(_MainTex, i.uv).rgb;
                half3 lerpCol = lerp(_BotCol.xyz, _TopCol.xyz, i.uv.y) * i.color;
                half3 baseCol = lerpCol * texCol;

                // fakeShadow
                half fakeShadow = lerp( saturate(floor(i.uv.x / _FakeShadowRange) + (1 - _FakeShadowIntensity)), 1.0, i.uv.y);

                // receive shadow
                half3 shadow = 1;
                #ifdef _MAIN_LIGHT_SHADOWS
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = i.posWS;
                    
                    half4 shadowCoord = GetShadowCoord(vertexInput);
                    half shadowAttenuation = saturate(MainLightRealtimeShadow(shadowCoord) + _ShadowIntensity);
                    shadow = lerp(0.0f, 1.0f, shadowAttenuation);
                #endif

                // Lambert
                Light mainLight = GetMainLight();
                half nl = max(saturate(dot(i.normal, mainLight.direction)), 0.00001);
                half nl_half = nl * 0.5 + 0.5;
                half3 diffuse = mainLight.color * baseCol * nl_half;

                col = diffuse * fakeShadow * shadow;
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass 
        {
            Name "GrassShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            HLSLPROGRAM

            // #pragma require geometry
            // #pragma require tessellation tessHW

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #pragma target 4.6
            #pragma multi_compile_shadowcaster

            half4 frag(geomdata i) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }
}
