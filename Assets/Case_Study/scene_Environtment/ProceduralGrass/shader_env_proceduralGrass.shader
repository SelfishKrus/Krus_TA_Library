Shader "Environment/ProceduralGrass"
{
    Properties
    {   
        [Header(Color)]
        _TopCol ("Top Color", Color) = (0,1,0,1)
        _BotCol ("Bottom Color", Color) = (0,0.5,0,1)
        [Space(30)]

        [Header(Shape Control)]
        _BendAmount ("Bend Amount", Range(0, 1)) = 0
        _BladeWidth ("Blade Width", Float) = 0.5
        _BladeHeight ("Blade Height", Float) = 1  
        _BladeWidthRand ("Blade Width Randomness", Range(0, 1)) = 0
        _BladeHeightRand ("Blade Height Randomness", Range(0, 1)) = 0
        [IntRange] _BladeSegNum ("Blade Segment Number", Range(1, 10)) = 3

        [Header(Wind)]
        _WindNoiseMap ("Wind Noise Map", 2D) = "white" {}
        _WindNoiseFrequency ("Wind Noise Frequency", Vector) = (0.5, 0.5, 0, 0)
        _WindStrength ("Wind Strength", Float) = 1

        _Test ("Test Factor", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" }
        Cull Off
        ZWrite On

        Pass
        {
            Tags 
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM

            #pragma require geometry
            #pragma require tessellation tessHW

            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain 
            #pragma geometry geom
            #pragma fragment frag

            #pragma target 4.6

            #define BLADE_SEG_NUM 3

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Common/Library/KrusNoise.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Test;
            half4 _TopCol;
            half4 _BotCol;

            half _BendAmount;
            half _BladeWidth;
            half _BladeHeight;
            half _BladeWidthRand;
            half _BladeHeightRand;
            int _BladeSegNum;

            sampler2D _WindNoiseMap;
            float4 _WindNoiseMap_ST;
            float2 _WindNoiseFrequency;
            float _WindStrength;
            CBUFFER_END

            // TEXTURE2D(_WindNoiseMap);   SAMPLER(sampler_WindNoiseMap);

            struct appdata
            {
                float4 posOS : POSITION;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half2 uv : TEXCOORD0;
            };

            struct tessdata
            {
                float4 pos : SV_POSITION;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half2 uv : TEXCOORD0;
                half3 barycentricCoordinates : TEXCOORD1;
            };

            struct vertdata
            {
                float4 pos : SV_POSITION;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half2 uv : TEXCOORD0;
                half3 barycentricCoordinates : TEXCOORD1;
            };

            struct geomdata
            {
                float4 pos : SV_POSITION;
                // half3 normal : NORMAL;
                // half4 tangent : TANGENT;
                half2 uv : TEXCOORD0;
            };

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            // CONSTANT FUNCTION
            TessellationFactors ConstantFunction(InputPatch<tessdata, 3> patch)
            {
                TessellationFactors f;
                f.edge[0] = _Test.x;
                f.edge[1] = _Test.x;
                f.edge[2] = _Test.x;
                f.inside = _Test.x;
                return f;
            }

            // FUNCTION START ////////////////////////////////////////////////////////

            // assign properties to vertices created in geom shader
            geomdata GenerateGrassVertices(float3 posOS, float3 offset, float3x3 matrix_transformation, half2 uv)
            {
                geomdata o;
                o.pos = TransformObjectToHClip(posOS + mul(matrix_transformation, offset));
                o.uv = uv;
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

            // VERT PROGRAM FOR TESSELLATION // 
            vertdata tessvert (tessdata v)
            {
                vertdata o;
                o.pos = v.pos;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.uv = v.uv;
                o.barycentricCoordinates = v.barycentricCoordinates;
                return o;
            }

            // VERT SHADER //
            tessdata vert (appdata v)
            {
                tessdata o;
                o.pos = v.posOS;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.uv = v.uv;
                return o;
            }

            // HULL SHADER //
            [domain("tri")]
            [partitioning("integer")]
            [outputtopology("triangle_cw")] 
            [outputcontrolpoints(3)]
            [patchconstantfunc("ConstantFunction")]
            tessdata hull(InputPatch<tessdata, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            // TESSELLATOR //

            // DOMAIN SHADER //
            [domain("tri")]
            vertdata domain(TessellationFactors factors, OutputPatch<tessdata, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                tessdata data;

                #define DOMAIN_INTERPOLATE(fieldName) data.fieldName = \
                    patch[0].fieldName * barycentricCoordinates.x + \
                    patch[1].fieldName * barycentricCoordinates.y + \
                    patch[2].fieldName * barycentricCoordinates.z;
                
                // interpolation
                DOMAIN_INTERPOLATE(pos)
                DOMAIN_INTERPOLATE(normal)
                DOMAIN_INTERPOLATE(tangent)
                DOMAIN_INTERPOLATE(uv)

                data.barycentricCoordinates = barycentricCoordinates;

                return tessvert(data);
            }

            // GEOM SHADER //
            [maxvertexcount(BLADE_SEG_NUM * 2 + 1)]
            void geom (triangle vertdata IN[3] : SV_POSITION, inout TriangleStream<geomdata> triStream)
            {
                // prepare variables
                float3 posOS = IN[0].pos.xyz ;
                half3 normal = IN[0].normal;
                half3 tangent = IN[0].tangent.xyz;
                half3 bitangent = cross(normal, tangent) * IN[0].tangent.w;


                // wind
                float2 windUV = posOS.xz * _WindNoiseMap_ST.xy + _WindNoiseMap_ST.zw + _WindNoiseFrequency * _Time.y;
                float2 windSample = (tex2Dlod(_WindNoiseMap, float4(windUV,0,0)).xy * 2 - 1) * _WindStrength;
                // rotation axis
                half3 windAxis = normalize(float3(windSample.x, windSample.y, 0));
                float3x3 matrix_windRotation = angleAxis3x3(PI * windSample.y, windAxis) ;

                // TBN matrix - TS to OS
                float3x3 matrix_TS2OS = transpose(float3x3(tangent, bitangent, normal));
                float3x3 matrix_randFaceRotation = angleAxis3x3(rand(posOS) * PI * 2, half3(0, 0, 1));
                float3x3 matrix_randBendRotation = angleAxis3x3(rand(posOS.zyx) * _BendAmount * PI * 0.5, half3(1, 0, 0));

                // final rotation matrix for base or top
                float3x3 matrix_transformation_tip = mul(mul(mul(matrix_TS2OS, matrix_windRotation), matrix_randFaceRotation), matrix_randBendRotation);
                float3x3 matrix_transformation_base = mul(matrix_TS2OS, matrix_randFaceRotation);

                // grass height & width
                half width = (rand(posOS.xyz) * 2 - 1) * _BladeWidthRand + _BladeWidth;
                half height = (rand(posOS.zyx) * 2 - 1) * _BladeHeightRand + _BladeHeight;

                // GENERATE VERTICES //
                // segment part
                for (int i = 0; i < BLADE_SEG_NUM; i++)
                {
                    float t = i / (float)BLADE_SEG_NUM;
                    // added vertices' pos info
                    float3 offset = float3(width * (1 - t), 0, height * t);
                    float3x3 matrix_transformation = (i == 0) ? matrix_transformation_base : matrix_transformation_tip;
                
                    triStream.Append(GenerateGrassVertices(posOS, float3(offset.x, offset.y, offset.z), matrix_transformation, float2(0, t)));
                    triStream.Append(GenerateGrassVertices(posOS, float3(-offset.x, offset.y, offset.z), matrix_transformation, float2(1, t)));
                    
                }
                
                // tip part
                triStream.Append(GenerateGrassVertices(posOS, float3(0, 0, height), matrix_transformation_tip, float2(0.5, 1)));
                

                triStream.RestartStrip();
            }

            // FRAG SHADER //
            half4 frag (geomdata i) : SV_Target
            {
                half3 col = lerp(_BotCol.rgb, _TopCol.rgb, i.uv.y);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
