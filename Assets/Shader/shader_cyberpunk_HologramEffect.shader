Shader "Cyberpunk/HologramEffect"
{
    Properties
    {
        _HologramCol ("Hologram_Color", Color) = (1,1,1,1) 
        _HologramAlpha ("Transparency", Range(0,1)) = 0.5

        _MainTex ("Main Texture", 2D) = "gray" {}
        [Normal] _NormalMap ("Normal Map", 2D) = "normal" {}
        _NormalScale ("Normal Scale", Range(0,10)) = 1
        _MaskEffect ("Mask Effect", Range(0,1)) = 0.5

        _TimeX ("Time", float) = 0
        _Speed ("Speed", Range(0,10)) = 3
        _LineWidth ("Line Width", float) = 1
        _Gamma ( "Gamma", float) = 4.4
        _Contrast ("Contrast", Range(0,10)) = 1

        _JitterDistance ("Jitter Distance", float) = 10

        _NoiseRatio("Noise Ration", Range(0,1)) = 0.5
        _RimRatio("Rim Ration", float) = 1
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "LightMode"="UniversalForward"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE

            float randomNoise(float2 xy)
            {
                return frac(sin(dot(xy, float2(123.353, 91.322))) *4353.5453);
            }

            float trunc( float x, float numLevels)
            {
                return floor(x * numLevels) / numLevels;
            }

            float3 VertexJitterOffset(float3 vertex, half jitterSpeedRatio, half jitterRangeY, half jitterOffset)
            {
                // periodical jitter 
                half offsetTime = sin(_Time.y * jitterSpeedRatio);
                half b_timeToJitter = step(0.99, offsetTime);

            }

            float JitterEffect(float3 worldPos, float jitterDistance)
                {
                    float t = frac(_Time.y + 0.7);
                    float jitterTime = trunc(t, 8);
                    float pos_streak = randomNoise(trunc(worldPos.y * 0.01, 12) + jitterTime);
                        // merge streak
                    float rangeToJitter = step(0.3, pos_streak);
                        // time to jitter 
                    float sinTime = sin(_Time.z);
                    half timeToJitter = step(0.98, sinTime);
                    float offset =  rangeToJitter * timeToJitter * jitterDistance;
                    return offset;
                }


        ENDCG

        Pass
        {
            ZWrite On
            ColorMask 0
        }

        Pass
        {
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 TBN1 : TEXCOORD1;
                float4 TBN2 : TEXCOORD2;
                float4 TBN3 : TEXCOORD3;
                
            };

            fixed4 _HologramCol;
            fixed _HologramAlpha;
            sampler2D _MainTex;
            sampler2D _NormalMap;
            float _NormalScale;
            float4 _MainTex_ST;
            fixed _MaskEffect;
            float _TimeX;
            float _Speed;
            float _LineWidth;
            float _Gamma;
            float _Contrast;
            float _JitterDistance;
            float _NoiseRatio;
            float _RimRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldBinormal = cross(worldNormal, worldTangent * v.tangent.w);
                
                // jitter //
                float offset = JitterEffect(worldPos, _JitterDistance);
                worldPos += float3(offset, 0, offset);
                o.pos = UnityWorldToClipPos(worldPos);

                // TBN //
                o.TBN1 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TBN2 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TBN3 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float3 worldPos = float3(i.TBN1.w, i.TBN2.w, i.TBN3.w);

                // normal //
                float3 normal_t = UnpackNormal(tex2D(_NormalMap, i.uv));
                normal_t.xy *= _NormalScale;
                normal_t.z = sqrt(1 - saturate(dot(normal_t.xy, normal_t.xy)));
                float3 worldNormal = normalize(float3(dot(i.TBN1.xyz, normal_t), dot(i.TBN2.xyz, normal_t), dot(i.TBN3.xyz, normal_t)));

                // preparation //
                float time = frac(_Time.y) * _Speed;
                float lineWidth = 1 / _LineWidth;
                float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float nv = max(dot(worldNormal, viewDir), 0.00001);

                // albedo //
                fixed3 albedo = tex2D(_MainTex, i.uv);

                // scan-line effect //
                float truncTime = trunc(time, 8);
                float pos_trunc = randomNoise(trunc(worldPos.y*_LineWidth, 2) + 100 * truncTime);
                // Random streak width
                pos_trunc = saturate(pos_trunc - 0.3);
                // Gamma Correction
                pos_trunc = pow(pos_trunc, _Gamma);


                // fresnel rim // 
                fixed3 F0 = unity_ColorSpaceDielectricSpec.rgb;
                fixed3 rim = F0 + (1 - F0) * pow(1 - nv, 5);
                rim *= _RimRatio;

                // flicker //
                float flicker = randomNoise(trunc(_Time.y * 0.5, 20));
                // the lowest luminance = 0.3
                flicker = lerp(0.5, 1.0, flicker);
                flicker = pow(flicker, 0.8);

                // noise //
                float noise = randomNoise(floor(i.uv * 1000) + truncTime);

                // mask //
                float mask = albedo.r  + pos_trunc;
                mask = lerp(1, mask, _MaskEffect);

                
                fixed4 col = 1;
                col.rgb =  _Contrast * lerp(_HologramCol + rim, noise, _NoiseRatio);
                col.a = _HologramAlpha * mask * flicker;

                col = 1;
                return col;
            }
            ENDCG
        }
    }
}
