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
            // #pragma require geometry
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #pragma target 4.6

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Common/Library/KrusNoise.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2g
            {
                float4 posOS : SV_POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _Test;
            half4 _TopCol;
            half4 _BotCol;

            half _BendAmount;
            half _BladeWidth;
            half _BladeHeight;
            half _BladeWidthRand;
            half _BladeHeightRand;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            // FUNCTION START ////////////////////////////////////////////////////////

            // assign properties to vertices created in geom shader
            g2f OutputVertex(float3 posOS, half2 uv)
            {
                g2f o;
                o.pos = TransformObjectToHClip(posOS);
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


            v2g vert (appdata v)
            {
                v2g o;
                o.posOS = v.posOS;
                o.normal = v.normal;
                o.tangent = v.tangent;
                return o;
            }

            [maxvertexcount(3)]
            void geom (triangle v2g IN[3] : SV_POSITION, inout TriangleStream<g2f> triStream)
            {
                // prepare variables
                float3 posOS = IN[0].posOS.xyz ;
                half3 normal = IN[0].normal;
                half3 tangent = IN[0].tangent.xyz;
                half3 bitangent = cross(normal, tangent) * IN[0].tangent.w;

                // TBN matrix - TS to OS
                float3x3 matrix_TS2OS = transpose(float3x3(tangent, bitangent, normal));
                float3x3 matrix_randFaceRotation = angleAxis3x3(rand(posOS) * PI * 2, half3(0, 0, 1));
                float3x3 matrix_randBendRotation = angleAxis3x3(rand(posOS.zyx) * _BendAmount * PI * 0.5, half3(1, 0, 0));

                float3x3 matrix_transformation = mul(mul(matrix_TS2OS, matrix_randFaceRotation), matrix_randBendRotation);

                // grass height & width
                half width = (rand(posOS.xyz) * 2 - 1) * _BladeWidthRand + _BladeWidth;
                half height = (rand(posOS.zyx) * 2 - 1) * _BladeHeightRand + _BladeHeight;

                // output vertices
                triStream.Append(OutputVertex( posOS + mul(matrix_transformation, float3(width, 0, 0)), float2(0, 0)));
                triStream.Append(OutputVertex(posOS + mul(matrix_transformation, float3(-width, 0, 0)), float2(1, 0)));
                triStream.Append(OutputVertex(posOS + mul(matrix_transformation, float3(0, 0, height)), float2(0.5, 1)));
                triStream.RestartStrip();
            }

            half4 frag (g2f i) : SV_Target
            {
                half3 col = lerp(_BotCol.rgb, _TopCol.rgb, i.uv.y);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
