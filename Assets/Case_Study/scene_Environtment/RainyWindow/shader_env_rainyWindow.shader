Shader "Rainy/RainyWindow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "" {}
        _Size ("Grid_Size", Range(1,10) ) = 1
        _Distortion("Distortion", Range(0, 10)) = 1
        _Blur ("Blur", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderPipeline"="UniversalPipeline"}
        LOD 100
        Cull Off 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags {"LightMode"="UniversalForward"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            
            // replace the field S with smoothstep
            #define S(a,b,t) smoothstep(a,b,t)

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 grabUV : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Size, _Distortion, _Blur;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.grabUV = ComputeGrabScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // random function from 0 to 1
            float N21(float2 p) {
                p = frac(p*float2(123.34, 345.34));
                p += dot(p, p+34.345);

                return frac(p.x*p.y);
                }

            // droplets layer
            float3 Layer(float2 UV, float t) {

                // grid aspect ratio
                float2 aspect = float2(2,1);
                float2 uv = UV;

                // repetition for droplet
                float2 uv_drop = (UV * _Size * aspect);
                uv_drop.y += t * 0.5;
                // random id
                float2 id = floor(uv_drop);
                // randomize t
                float n = N21(id);
                t += n*6.28;
                uv_drop = frac(uv_drop) - 0.5;
                // repetition for trail
                float2 uv_trail = (UV * _Size * aspect);
                uv_trail.y *= 8;
                uv_trail = frac(uv_trail) - 0.5;
                
                float w = UV.y * 10;
                float x = ( n - 0.5 ) * 0.8; // (-0.4, 0.4)
                x += (0.4 - abs(x)) * sin(3*w) * pow(sin(w), 6) * 0.45;
                float y = -sin(t + sin(t+sin(t))) * 0.45;
                y -= pow(uv_drop.x - x, 2);

                float2 dropPos = (uv_drop - float2(x,y)) / aspect;
                float2 trailPos = (uv_trail - float2(x,y)) / aspect;
                trailPos.y = (trailPos.y - 0.1) / 8;

                // draw droplets
                float droplet = S(.05,.03,length(dropPos));
                
                // draw the trail
                float trail = S(.03,.01,length(trailPos));
                // remove the trail beneath the droplet
                float fogTrail = S(-.03, .03, dropPos.y);
                //gradiation
                fogTrail *= S(0.5, 0, uv_drop.y);
                trail *= fogTrail;

                fogTrail *= S(.05, .01, abs(dropPos.x));

                // col += fogTrail*0.5;
                // col += droplet;
                // col += trail;

                float2 offset = droplet * dropPos + trail * trailPos;
                // (1-fogTrail) means the place that droplets flow by keeps clear

                return float3(offset, fogTrail);
                }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = fmod(_Time.y * 0.9, 7200);

                float4 col;
                
                float3 droplets = Layer(i.uv, t);
                droplets += Layer(i.uv * 1.2 + 3.5, t);
                droplets += Layer(i.uv * 1.6 - 1.5, t);

                float blur = _Blur /100 * (1-droplets.z);
                // col = tex2Dlod(_MainTex, float4(i.uv + droplets.xy * _Distortion, 0, blur));

                float2 projUV = i.grabUV.xy / i.grabUV.w;
                // distortion
                projUV += droplets.xy * _Distortion;

                const int numSamples = 20;
                // Gaussian Sample index
                float a = N21(i.uv)*6.28;
                for (int i = 0; i < numSamples; i++) {

                    float2 offs = float2(sin(a), cos(a)) * blur;
                    // make offset more randomly
                    offs *= frac(sin(i+1.2)*13.5)*34.3;
                    col += tex2D(_MainTex, projUV+offs);
                    a++;

                    }

                col /= numSamples;
                //col = tex2D(_CameraImage, i.uv);

                //col *= 0; col.r = fogTrail;
                // red line on border
                // if ( uv_drop.x > 0.48 || uv_drop.y > 0.49) col = float4(1,0,0,1);
                return col;
            }
            ENDCG
        }
    }
}
