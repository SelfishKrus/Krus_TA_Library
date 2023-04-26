Shader "CharacterRendering/Skin"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _BaseTex ("Base Color", 2D) = "" {}
        _NormalTex ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0, 3)) = 1.0
        _MetallicTex ("Metallic Map", 2D) = "black" {}
        _RoughnessTex ("Roughness Map", 2D) = "white" {}
        _RoughnessScale ("Roughness Scale", Range(0, 5)) = 1.0
        _AOTex ("AO Map", 2D) = "white" {}
        _AOScale ("AO Scale", Range(0, 3)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Common/Shader/SJSJPBR.hlsl"

            struct appdata
            {
                float4 posOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 TBN0 : TEXCOORD1;
                float4 TBN1 : TEXCOORD2;
                float4 TBN2 : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.posOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseTex);

                float3 posWS = TransformObjectToWorld(v.posOS.xyz);
                half3 normalWS = TransformObjectToWorldNormal(v.normalOS.xyz, true);
                half3 tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz, true);
                half3 binormalWS = cross(normalWS, tangentWS) * v.tangentOS.w;

                // TBN matrix
                o.TBN0 = float4(tangentWS.x, binormalWS.x, normalWS.x, posWS.x);
                o.TBN1 = float4(tangentWS.y, binormalWS.y, normalWS.y, posWS.y);
                o.TBN2 = float4(tangentWS.z, binormalWS.z, normalWS.z, posWS.z);
                
                // shadowmap sampling coordinate
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.posOS.xyz);
                o.shadowCoord = GetShadowCoord(vertexInput);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // === VARIABLE PREPARATION === //
                // world normal
                half4 normalCol = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);
                half3 normalTS = UnpackNormalScale(normalCol, _NormalScale);
                half3 normalWS = normalize(half3(dot(i.TBN0.xyz, normalTS), dot(i.TBN1.xyz, normalTS), dot(i.TBN2.xyz, normalTS)));
                
                // Masks decompression
                // r - metallic, g - AO, b - roughness
                half metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, i.uv).r;
                half AO = clamp(SAMPLE_TEXTURE2D(_AOTex, sampler_AOTex, i.uv).r, 0, 1);
                AO = pow(AO, _AOScale);
                half tempRoughness = SAMPLE_TEXTURE2D(_RoughnessTex, sampler_RoughnessTex, i.uv).r;
                tempRoughness = saturate(tempRoughness * _RoughnessScale);
                half roughness = pow(tempRoughness, 2);
                half smoothness = 1 - tempRoughness;
                
                half3 baseCol = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv).rgb;
                half alpha = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv).a;
                baseCol *= _Tint.rgb;
                half3 F0 = lerp(0.04, baseCol, metallic);

                float3 posWS = float3(i.TBN0.w, i.TBN1.w, i.TBN2.w);
                Light mainLight = GetMainLight(i.shadowCoord);
                half3 n = normalWS;
                half3 l = normalize(mainLight.direction);
                half3 v = SafeNormalize(_WorldSpaceCameraPos - posWS);
                half3 h = normalize(v+l);
                half nv = max(saturate(dot(n, v)), 0.00001);
                half nl = max(saturate(dot(n, l)), 0.00001);
                half hv = max(saturate(dot(h, v)), 0.00001);
                half nh = max(saturate(dot(n, h)), 0.00001);
                half lh = max(saturate(dot(l, h)), 0.00001);

                // === DIRECT LIGHT === //
                // Specular - Cook-Torrance
                float D = GetD(nh, roughness);
                float G = GetG(nv, nl, roughness);
                float3 F_direct = GetF_DL(F0.r, max(hv,0));
                float3 BRDF_direct_spec = D * G * F_direct / ( 4*nv*nl );
                float3 SpecCol_direct = BRDF_direct_spec * mainLight.color * nl * PI;   // to compensate for PI igonred in diffuse part
                // Diffuse - Lambert
                float3 k_s_direct = F_direct;
                float3 k_d_direct = (1 - k_s_direct) * (1 - metallic);
                float3 DiffCol_direct = k_d_direct * baseCol * mainLight.color * nl;
                // Color from direct light
                float3 DirectCol = DiffCol_direct + SpecCol_direct;
                
                half3 col = DirectCol * AO;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
