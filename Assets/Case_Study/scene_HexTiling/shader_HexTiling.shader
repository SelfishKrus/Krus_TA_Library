Shader "KrusShader/HexTiling"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HexSize ("Hex Size", Float) = 0.1
        _EdgeContrast ("Edge Contrast", Float) = 1

        _Test ("Test", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" "IgnoreProjector"="True"}
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Case_Study/scene_HexTiling/TextureTilingRandomization.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };
            
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _HexSize;
            float _EdgeContrast;

            float4 _Test;

            TEXTURE2D(_MainTex);                SAMPLER(sampler_MainTex);

            /////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////    Function   ///////////////////////////////
            /////////////////////////////////////////////////////////////////////////////

            float3 getcolorfromint(int2 p)
            {
                int m = p.x + p.y * 2; 
                int n = abs(m) % 3;  
                if (m < 0)
                {             
                    if (n == 0)
                        return float3(1, 0, 0);
                    else if (n == 1)
                        return float3(0, 1, 0);
                    else
                        return float3(0, 0, 1);
                }
                else
                {
                    if (n == 0)
                        return float3(1, 0, 0);
                    else if (n == 1)
                        return float3(0, 0, 1);
                    else
                        return float3(0, 1, 0);
                }
            }

            // float2x2 rot2(int2 p)
            // {                
            //     float angle = _Rotation / 180 * 3.1415926 + abs(p.x * p.y) + abs(p.x + p.y);
            //     float cs = cos(angle);
            //     float sn = sin(angle);
            //     float2x2 matRot = float2x2(
            //         cs, -sn,
            //         sn, cs
            //     );
            //     return matRot;
            // }

            // FUNCTION END 

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                float2 uv = i.uv;
                float hexSize = _HexSize;
                float edgeContrast = _EdgeContrast;
                float lod = 0;
                
                float4 result = SampleTextureRandomizedHexGrid(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), uv, 0, hexSize, edgeContrast);
                
                // // we want UVs in hexGrid UV space, as they are integer and more stable as hash random seeds
                // TrianglePoint tri = GetHexGridTriangle(uv, hexSize, true);

                // // randomize transform at each vertex (identified by coord)
                // TriangleUV triUV;
                // triUV.r = RandomScaleOffsetUV(uv, 0.8f, 1.2f, GridRandom3(tri.vertexCoords.r));
                // triUV.g = RandomScaleOffsetUV(uv, 0.8f, 1.2f, GridRandom3(tri.vertexCoords.g));
                // triUV.b = RandomScaleOffsetUV(uv, 0.8f, 1.2f, GridRandom3(tri.vertexCoords.b));

                // // (optional) contrast enhance the blend -- should ideally turn this off at distance to avoid aliasing
                // tri.weights = pow(tri.weights, edgeContrast);

                // // normalize weights.  Not necessary if detail texture and contrast enhance are disabled.
                // tri.weights = tri.weights / dot(tri.weights, 1.0f);

                // // sample material for each triangle corner
                // TriangleFloat4 triValues = SampleTriangleTextures(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), triUV, lod);

                // // blend the material samples based on the weights
                // float4 result = BlendWithTriangleWeights(tri.weights, triValues);


                half3 col;
                col = result;

                return half4(col , 1);
            }
            ENDHLSL
        }
    }
}
