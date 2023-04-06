Shader "Common/Tessellation"
{
    Properties
    {
        _BaseCol ("Base Color", Color) = (1,1,1,1)
        [Space(10)]
        
        [Header(Wireframe)]
        _WireframeThickness ("Width", Range(0, 10)) = 1
        _WireframeSmoothing("Smoothness", Range(0, 10)) = 1
        _WireframeCol ("Wireframe Color", Color) = (0,0,0,1)
        [Space(10)]

        [Toggle] _Alpha_Test ("Enable Aplpha Test", Float) = 0

        _Test ("Test", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM 
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma geometry geom
            #pragma fragment frag

            #pragma require geometry
            #pragma require tessellation tessHW

            #pragma multi_compile_local __ _ALPHA_TEST_ON

            // needed
            #pragma target 4.6

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseCol;

            float _WireframeThickness;
            float _WireframeSmoothing;
            half3 _WireframeCol;

            float4 _Test;
            CBUFFER_END

            // FUNCTION START ////////////////////////////////////////////////////
            half3 DrawWireFramesByBary(half3 barycentricCoordinates, half3 wireframeCol, half3 baseCol)
            {
                float3 deltas = fwidth(barycentricCoordinates);

                float3 smoothing = deltas * _WireframeSmoothing;
	            float3 thickness = deltas * _WireframeThickness;
	            half3 barys = smoothstep(thickness, thickness + smoothing, barycentricCoordinates);
                // to remove margin artefacts
                half minBary = min(barys.x, min(barys.y, barys.z));
    
                half3 col = lerp(wireframeCol, baseCol, minBary);
                return col;
            }

            half3 DrawWireFramesByStep(float _Width, half2 uv)
            {
                float lowX = step(_Width, uv.x);
                float lowY = step(_Width, uv.y);
                float highX = step(uv.x, 1 - _Width);
                float highY = step(uv.y, 1 - _Width);
                float edge = lowX * lowY * highX * highY;

                return edge;
            }
            // FUNCTION END ////////////////////////////////////////////////////

            struct appdata
            {
                float4 posOS : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            // output of the first vert shader
            // control points of tessllation shader
            struct tessdata
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv :TEXCOORD0;
            };

            struct vertdata
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                half3 barycentricCoordinates : TEXCOORD1;
            };
            
            struct geodata
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half3 barycentricCoordinates : TEXCOORD1;
            };

            // output of ConstantFunction
            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            // Constant functin 
            // invoked once per patch
            // to specify how many parts the patch should be divided into
            TessellationFactors ConstantFunction(InputPatch<tessdata, 3> patch)
            {
                TessellationFactors f;
                f.edge[0] = _Test.x;
                f.edge[1] = _Test.x;
                f.edge[2] = _Test.x;
                f.inside = _Test.x;
                return f;
            }

            
            // ?????????
            // VERTEX PROGRAMMING FOR TESSELLATION //
            tessdata tessVert(appdata v)
            {
                tessdata o;
                o.pos = v.posOS;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.uv = v.uv;
                return o;
            }

            // VERTEX SHADER // 
            tessdata vert (appdata v)
            {
                tessdata o;
                o.pos = v.posOS;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.uv = v.uv;
                return o;
            }


            // HULL SHADER (Tessellation control shader) //
            // evoked once per vertex in a patch
            // to break high-level mesh into a series of smaller patches and pass them to tessellation shader
            [domain("tri")]     // work with triangle
            [outputcontrolpoints(3)]    // output 3 control points per patch
            [outputtopology("triangle_cw")]     // output triangle with clockwise winding
            [partitioning("fractional_even")]   // integer partitioning mode
            [patchconstantfunc("ConstantFunction")]    // use constant function to calculate patch constant data
            tessdata hull(InputPatch<tessdata, 3> patch, uint id : SV_OutputControlPointID)
            {

                return patch[id];
            }

            // TESSELLATION SHADER // 
            // generate baycentric coordinates for vertices
            
            // DOMAIN SHADER (Tessellation evaluation shader) // 
            // invoked once per vertex in a patch
            // use baycentric coordinates to generate vertices
            [domain("tri")]
            vertdata domain(TessellationFactors factors, OutputPatch<tessdata, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                vertdata data;

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

                return data;
            }

            // GEOMETRY SHADER //
            [maxvertexcount(3)]
            void geom (triangle vertdata v[3], inout TriangleStream<geodata> triStream)
            {
                for (int i = 0; i < 3; i++)
                {
                    geodata o;
                    o.pos = TransformObjectToHClip(v[i].pos.xyz);
                    o.uv = v[i].uv;
                    o.barycentricCoordinates = v[i].barycentricCoordinates;
                    triStream.Append(o);
                }
                triStream.RestartStrip();
            }


            // FRAGMENT SHADER // 
            half4 frag (geodata i) : SV_Target
            {
                // draw wireframes
                // half edge = DrawWireFramesByStep(_Width, i.uv);

                half3 wireframe = DrawWireFramesByBary(i.barycentricCoordinates, _WireframeCol, _BaseCol.rgb);
                
                #if _ALPHA_TEST_ON
                    clip(0.01 - wireframe);
                #endif

                half3 col = wireframe;
                return half4(col, _BaseCol.a);
            }
            ENDHLSL
        }
    }
}
