Shader "Common/Tessellation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // #pragma geometry geom
            #pragma fragment frag

            // needed
            #pragma target 4.6

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct hull_in
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            struct frag_in
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            // Hull shader
            [domain("tri")]     // work with triangle
            [outputcontrolpoints(3)]    // output 3 control points per patch
            [outputtopology("triangle_cw")]     // output triangle with clockwise winding
            [partitioning("integer")]   // integer partitioning mode
            [patchconstantfunc("constant")]    // use constant function to calculate patch constant data
            hull_in hull(InputPatch<hull_in, 3> patch, uint id : SV_OutputControlPointID)
            {

                return patch[id];
            }


            hull_in vert (appdata v)
            {
                hull_in o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (frag_in i) : SV_Target
            {
                
                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
