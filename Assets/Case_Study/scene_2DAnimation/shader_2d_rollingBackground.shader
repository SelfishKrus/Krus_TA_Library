Shader "2DAnimation/rollingBackground"
{
    Properties
    {
        _Foreground ("Foreground", 2D) = "white" {}
        _Background ("Background", 2D) = "white" {}
        _Speed_F ("Foregournd Speed", Range(0,1)) = 0.2
        _Speed_B ("Backgournd Speed", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _Foreground;
            float4 _Foreground_ST;
            sampler2D _Background;
            float4 _Background_ST;
            float _Speed_F;
            float _Speed_B;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _Foreground);
                o.uv.x += frac(_Speed_F * _Time.y);
                o.uv.zw = TRANSFORM_TEX(v.uv, _Background);
                o.uv.z += frac(_Speed_B * _Time.y);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                

                // sample the texture
                fixed4 fCol = tex2D(_Foreground, i.uv.xy);
                fixed4 bCol = tex2D(_Background, i.uv.zw);
                fixed4 col = lerp(bCol, fCol, fCol.a);
                return col;
            }
            ENDCG
        }
    }
}
