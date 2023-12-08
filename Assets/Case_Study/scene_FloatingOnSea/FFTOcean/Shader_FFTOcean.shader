Shader "KrusShader/FFTOcean"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        [Toggle(UV_POS_OS)] _UV_POS_OS ("Enable UV POS OS", Float) = 0
        _Test ("Test", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent"}


        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag

            #pragma require tessellation tessHW

            #pragma multi_compile _ UV_POS_OS


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct tessdata
            {
                float4 posOS : POSITION;
                half3 normalWS : NORMAL;
                half4 tangentWS : TANGENT;
                float2 uv : TEXCOORD0;
                half3 barycentricCoordinates : TEXCOORD1;
                half3 posWS : TEXCOORD2;
            };

            struct vertdata
            {
                float4 pos : SV_POSITION;
                half3 normalWS : NORMAL;
                half4 tangentWS : TANGENT;
                half2 uv : TEXCOORD0;
                half3 barycentricCoordinates : TEXCOORD1;
                float3 posWS : TEXCOORD2;
                float3 posOS : TEXCOORD3;
            };

            float _TileGlobalScale;
            float _GlobalScale;
            float _TessellationFactor;

            float4 _MainTex_ST;
            float4 _Test;

            float _Tile0, _Tile1, _Tile2, _Tile3;
			int _DebugTile0, _DebugTile1, _DebugTile2, _DebugTile3;
			int _DebugLayer0, _DebugLayer1, _DebugLayer2, _DebugLayer3;
			int _ContributeDisplacement0, _ContributeDisplacement1, _ContributeDisplacement2, _ContributeDisplacement3;


            TEXTURE2D_ARRAY(_DisplacementTextures);    SAMPLER(sampler_DisplacementTextures);
            TEXTURE2D_ARRAY(_SlopeTextures);    SAMPLER(sampler_SlopeTextures);
            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////              VERTEX SHADER                /////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////

            tessdata vert(appdata v)
            {
                tessdata o;

                o.posOS = v.posOS;
                o.normalWS = TransformObjectToWorldDir(v.normalOS);
                o.tangentWS.xyz = TransformObjectToWorldDir(v.tangentOS);
                o.posWS = TransformObjectToWorld(v.posOS).xyz;
                o.uv = v.uv;
                return o;
            }

            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////                HULL SHADER                /////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            // Constant function
            // invoked once per patch
            // to specify how many parts the patch should be divided into
            TessellationFactors ConstantFunction(InputPatch<appdata, 3> patch)
            {
                TessellationFactors f;
                f.edge[0] = _TessellationFactor;
                f.edge[1] = _TessellationFactor;
                f.edge[2] = _TessellationFactor;
                f.inside = _TessellationFactor;
                return f;
            }

            // evoked once per vertex in a patch
            // to break high-level mesh into a series of smaller patches and pass them to tessellation shader
            [domain("tri")]     // work with triangle
            [outputcontrolpoints(3)]    // output 3 control points per patch
            [outputtopology("triangle_cw")]     // output triangle with clockwise winding
            [partitioning("fractional_even")]   // integer partitioning mode
            [patchconstantfunc("ConstantFunction")]    // use constant function to calculate patch constant data
            appdata hull(InputPatch<appdata, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////      TESSELLATION SHADER  (On Hardware)   /////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////               DOMAIN SHADER               /////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////

            vertdata tessvert(tessdata v)
            {
                vertdata o;
                o.pos = TransformObjectToHClip(v.posOS);
                o.normalWS = normalize(v.normalWS);
                o.tangentWS = v.tangentWS;
                o.uv = v.uv;
                o.barycentricCoordinates = v.barycentricCoordinates;
                o.posWS = v.posWS;
                o.posOS = v.posOS;
                return o;
            }

            // invoked once per vertex in a patch
            // use baycentric coordinates to generate vertices
            [domain("tri")]
            vertdata domain(TessellationFactors factors, OutputPatch<tessdata, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                tessdata data;

                #define DOMAIN_INTERPOLATE(fieldName) data.fieldName = \
                    patch[0].fieldName * barycentricCoordinates.x + \
                    patch[1].fieldName * barycentricCoordinates.y + \
                    patch[2].fieldName * barycentricCoordinates.z;
                
                // interpolation
                DOMAIN_INTERPOLATE(posOS)
                DOMAIN_INTERPOLATE(posWS)
                DOMAIN_INTERPOLATE(normalWS)
                DOMAIN_INTERPOLATE(tangentWS)
                DOMAIN_INTERPOLATE(uv)

                data.barycentricCoordinates = barycentricCoordinates;

                #ifdef UV_POS_OS
                    float2 uv = float2(data.posOS.x, data.posOS.z) / _TileGlobalScale ;
                #else
                    float2 uv = data.uv;
                #endif

                half3 displacement1 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, uv * _Tile0, 0, 0) * _DebugLayer0 * _ContributeDisplacement0;
                half3 displacement2 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, (uv - 0.5f) * _Tile1, 1, 0) * _DebugLayer1 * _ContributeDisplacement1;
                half3 displacement3 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, (uv - 1.125f) * _Tile2, 2, 0) * _DebugLayer2 * _ContributeDisplacement2;
                half3 displacement4 = SAMPLE_TEXTURE2D_ARRAY_LOD(_DisplacementTextures, sampler_DisplacementTextures, (uv - 1.25f) * _Tile3, 3, 0) * _DebugLayer3 * _ContributeDisplacement3;
				half3 displacement = displacement1 + displacement2 + displacement3;

                // displace in tangent space
                float3 tangentOS = TransformWorldToObjectDir(data.tangentWS.xyz);
                float3 normalOS = TransformWorldToObjectDir(data.normalWS);
                float3 bitangentOS = cross(tangentOS, normalOS);
                float3x3 matrix_TangentToObject = float3x3(
                    tangentOS,
                    normalOS,
                    bitangentOS
                );

                data.posOS.xyz += mul(matrix_TangentToObject ,displacement * _GlobalScale * 0.001);

                return tessvert(data);
            }

            //////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////              FRAGMENT SHADER              /////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////

            half4 frag (vertdata i) : SV_Target
            {
                Light mainLight;
                mainLight = GetMainLight();

                // uv by posOS
                #ifdef UV_POS_OS
                    float2 uv = float2(i.posOS.x, i.posOS.z) / _TileGlobalScale;
                #else
                    float2 uv = i.uv;
                #endif

                half3 displacement1 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, uv * _Tile0, 0) * _DebugLayer0 * _ContributeDisplacement0;
                half3 displacement2 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, (uv - 0.5f) * _Tile1, 1) * _DebugLayer1 * _ContributeDisplacement1;
                half3 displacement3 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, (uv - 1.125f) * _Tile2, 2) * _DebugLayer2 * _ContributeDisplacement2;
                half3 displacement4 = SAMPLE_TEXTURE2D_ARRAY(_DisplacementTextures, sampler_DisplacementTextures, (uv - 1.25f) * _Tile3, 3) * _DebugLayer3 * _ContributeDisplacement3;
				half3 displacement = displacement1 + displacement2 + displacement3;

                float2 slopes1 = SAMPLE_TEXTURE2D_ARRAY(_SlopeTextures, sampler_SlopeTextures, uv * _Tile0, 0) * _DebugLayer0;
				float2 slopes2 = SAMPLE_TEXTURE2D_ARRAY(_SlopeTextures, sampler_SlopeTextures, (uv - 0.5) * _Tile1, 1) * _DebugLayer1;
				float2 slopes3 = SAMPLE_TEXTURE2D_ARRAY(_SlopeTextures, sampler_SlopeTextures, (uv - 1.125) * _Tile2, 2) * _DebugLayer2;
				// float2 slopes4 = SAMPLE_TEXTURE2D_ARRAYY(_SlopeTextures, sampler__SlopeTextures, (uv - 1.25) * _Tile3, 3) * _DebugLayer3;
				float2 slopes = slopes1 + slopes2 + slopes3;

                // Lambert
				float3 normalWS = normalize(float3(-slopes.x, 1.0f, -slopes.y));
                half3 diff = dot(normalWS, mainLight.direction);

                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                col = displacement;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
