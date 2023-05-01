Shader "2DAnimation/PixelCat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HAmount ("Horizontal Amount", float) = 9
        _VAmount ("Vertical Amount", float) = 1
        _Speed ("Speed", Range(0,10)) = 1
        _TimeOffset ("Time Offset", Range(0,10)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HAmount;
            float _VAmount;
            float _Speed;
            float _TimeOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float time = floor( _TimeOffset + _Time.y * _Speed);

                i.uv.x /= _HAmount;
                i.uv.y /= _VAmount;

                float row = floor(time / _HAmount);
                float column = fmod(time, _HAmount);
                
                i.uv.x += column / _HAmount;
                i.uv.y += row / _VAmount;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // fixed4 c = fixed4(0,0,0,1);
                // c.r = column / _VAmount;
                return col;
            }
            ENDCG
        }
    }
}
