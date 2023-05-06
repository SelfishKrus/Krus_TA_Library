Shader "CharacterRendering/Hair_KajiyaKay"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        [NoScaleOffset] _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        [NoScaleOffset] _MetallicTex ("Metallic Map", 2D) = "black" {}
        [NoScaleOffset] _RoughnessTex ("Roughness Map", 2D) = "" {}
        _RoughnessScale ("Roughness Scale", Range(0, 1)) = 1.0
        [NoScaleOffset] _AOTex ("AO Map", 2D) = "white" {}
        _AOScale ("AO Scale", Range(0, 3)) = 1.0

        _ShiftTangentTex ("Shift Tangent Map", 2D) = "white" {}
        _SpecCol1 ("Specular Color 1", Color) = (1,0,0,1)
        _SpecCol2 ("Specular Color 2", Color) = (0,0,1,1)
        _Shift ("Shift", Vector) = (0.5,1,0,0)
        _Glossiness ("Glossiness", Vector) = (1, 1, 0, 0)

        _TestFactor ("Test Factor", float) = 0.5
    }
    SubShader
    {   
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert_PBR
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/Common/Shader/SJSJ_PBRSetup.hlsl"
            #include "Assets/Common/Shader/SJSJ_BRDF.hlsl"

            half3 _SpecCol1;
            half3 _SpecCol2;
            sampler2D _ShiftTangentTex;
            half2 _Shift;
            half2 _Glossiness;

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

            // FUNCTION START ////////////////////////////////////////////////

            half StrandSpecular (half3 t, half3 v, half3 l, half exponent) {
                half3 h = normalize(l + v);
                half th = dot(t, h);
                half sin_th = sqrt(1 - th * th);
                half dirAtten = smoothstep(-1, 0, th);

                return dirAtten * pow(sin_th, exponent);
            }

            half3 ShiftTangent (half3 t, half3 n, half shift) {
                half3 shiftedT = t + shift * n;
                return normalize(shiftedT);
            }

            // FUNCTION END /////////////////////////////////////////////////

            half4 frag (v2f_PBR i) : SV_Target
            {
                // === VARIABLE PREPARATION === //
                PBR_setup pbr_setup;
                SetupPBRVariables(i, pbr_setup);

                PBR_vectors pbr_vectors;
                SetupPBRVectors(i, pbr_vectors);

                // Get direct light info
                Light mainLight = GetMainLight(i.shadowCoord);

                half3 tangentWS = {i.TBN0.x, i.TBN1.x, i.TBN2.x};
                half3 bitangentWS = {i.TBN0.y, i.TBN1.y, i.TBN2.y};

                // === DIRECT LIGHT === //
                // Diffuse // 
                half3 diffCol_DL = pbr_setup.baseCol * lerp(0.25, 1.0, pbr_vectors.nl);
                // Specular // 
                half shiftTex = tex2D(_ShiftTangentTex, i.uv).r - 0.5;
                half3 t1 = ShiftTangent(tangentWS, pbr_vectors.n, _Shift.x + shiftTex);
                half3 t2 = ShiftTangent(tangentWS, pbr_vectors.n, _Shift.y + shiftTex);

                half3 specCol_DL = _SpecCol1 * StrandSpecular(t1, pbr_vectors.v, pbr_vectors.l, _Glossiness.x)
                                    + _SpecCol2 * StrandSpecular(t2, pbr_vectors.v, pbr_vectors.l, _Glossiness.y);


                half3 col;
                col = (diffCol_DL + specCol_DL) * mainLight.color;
                half alpha = tex2D(_BaseTex, i.uv).a;
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
