Shader "PostProcessing/RadialDistortion"
{
    Properties
    {   
        [HideInInspector] _Center ("Center", Vector) = (0.5,0.5,0,0)
        _NoiseMap ("Noise Map", 2D) = "white" {}
        _ScaleUV ("Scale UV", Vector) = (1,1,0,0) 
        _Speed ("Speed", Range(0,1)) = 0.2

        _DistortionRange ("Distortion Range", Range(0, 1)) = 0.6
        _DistrotionIntensity ("Distortion Intensity", Float) = 3

        [HideInInspector] _MainTex("Texture", 2D) = "" {}

        [Space(30)]
        _Test ("Test Factor", Vector) = (0,0,0,0)
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}
        LOD 100

        HLSLINCLUDE

        // FUNCTION START //////////////////////////////////////////////////////////

        // random noise
        inline float rand(half2 uv)
        {
            return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
        }

        // polar UV 
        float2 Unity_PolarCoordinates_float(float2 UV, float2 Center, float RadialScale, float LengthScale)
        {
            float2 delta = UV - Center;
            float radius = length(delta) * 2 * RadialScale;
            float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * LengthScale;
            return float2(radius, angle);
        }

        // FUNCTION END ////////////////////////////////////////////////////////////
        ENDHLSL

        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            half2 _Center;
            sampler2D _NoiseMap;
            half2 _ScaleUV;
            half _DistortionRange;
            half _DistrotionIntensity;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Speed;

            half4 _Test;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                half time = _Time.y * _Speed;

                // MASK FOR OFFSET//
                // draw a circle
                half dst = length(i.uv - _Center) * 2;
                half innerRadius = 0;
                half outerRadius = (1 - _DistortionRange) * 5;    // dirty remapping
                half intensity = max(_DistrotionIntensity, 0);
                half mask = smoothstep(innerRadius, outerRadius, dst) * intensity;

                /*
                half circle = 1 - dst;
                half circleFrac = frac(circle + time);
                half circleContinuous = saturate( sin(circleFrac * _Frequency * PI * 2) + _RippleIntensity );
                half mask_ripple = circleContinuous; 
                */

                // noise
                half2 polarUV = Unity_PolarCoordinates_float(i.uv, half2(0.5,0.5), _ScaleUV.x, _ScaleUV.y);
                half3 noiseCol_0 = tex2D(_NoiseMap, polarUV + time).rgb;
                half3 noiseCol_1 = tex2D(_NoiseMap, polarUV - time).rgb;

                half offsetX = ((noiseCol_0.r + noiseCol_1.r) - 1) ;
                half offsetY = ((noiseCol_0.g + noiseCol_1.g) - 1) ;
                half2 offset = half2(offsetX, offsetY);
                
                // lerp uv between distorted and original
                //prevention from uv going out of bounds
                half2 FlowUV = ((i.uv + offset > 1)) ? i.uv : i.uv + offset;
                half2 uv = lerp(i.uv, FlowUV, mask);

                half3 camCol = tex2D(_MainTex, uv).rgb;

                half3 col = camCol;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
