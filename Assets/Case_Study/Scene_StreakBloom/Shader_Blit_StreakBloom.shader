Shader "Blit/StreakBloom" 
{   
    Properties
    {
        _Threshold ("Threshold", Range(0, 5)) = 0.5
        _Stretch ("Stretch", Range(0, 5)) = 0.5
        _Intensity ("Intensity", Range(0, 5)) = 0.5
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    // The Blit.hlsl file provides the vertex shader (Vert),
    // input structure (Attributes) and output strucutre (Varyings)
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    TEXTURE2D_X(_CamColTex);
    TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
    TEXTURE2D(_InputTex);   SAMPLER(sampler_InputTex);
    TEXTURE2D(_HighTex);   SAMPLER(sampler_HighTex);

    float4 _InputTex_TexelSize;

    float _Threshold;
    float _Stretch;
    float _Intensity;
    float3 _Color;

    // Prefilter
    // Filter out bright pixels
    half4 FragPrefilter (Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 uvSS = input.texcoord * _ScreenParams.xy;
        
        float3 col = LOAD_TEXTURE2D_X(_CamColTex, uvSS).rgb;
        // float bright = max(col.r, max(col.g, col.b));
        // col *= max(0, bright - _Threshold) / max(bright, 1e-5);
        col *= max(0, col - _Threshold) / max(col, 1e-5);

        return float4(col, 1);
    }

    // Downsampler
    // Blur horizontally
    // sample points closer to the center have more weight
    float4 FragDownsample (Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.texcoord;
        const float dx = _InputTex_TexelSize.x;

        float u0 = uv.x - dx * 5;
        float u1 = uv.x - dx * 3;
        float u2 = uv.x - dx * 1;
        float u3 = uv.x + dx * 1;
        float u4 = uv.x + dx * 3;
        float u5 = uv.x + dx * 5;

        half3 c0 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u0, uv.y)).rgb;
        half3 c1 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u1, uv.y)).rgb;
        half3 c2 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u2, uv.y)).rgb;
        half3 c3 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u3, uv.y)).rgb;
        half3 c4 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u4, uv.y)).rgb;
        half3 c5 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u5, uv.y)).rgb;

        return half4((c0 + c1 * 2 + c2 * 3 + c3 * 3 + c4 * 2 + c5) / 12, 1);
        
    }

    // Upsampler
    // _InputTexture - downsampled texture
    // _HighTexture - blurred original texture
    float4 FragUpsample(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.texcoord;
        const float dx = _InputTex_TexelSize.x * 1.5;

        float u0 = uv.x - dx;
        float u1 = uv.x;
        float u2 = uv.x + dx;

        float3 c0 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u0, uv.y)).rgb;
        float3 c1 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u1, uv.y)).rgb;
        float3 c2 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u2, uv.y)).rgb;
        float3 c3 = SAMPLE_TEXTURE2D(_HighTex,  sampler_HighTex, uv).rgb;

        return float4(lerp(c3, c0 / 4 + c1 / 2 + c2 / 4, _Stretch), 1);

        // test 
        // return float4(c0 / 4 + c1 / 2 + c2 / 4, 1);
        // return float4(c3, 1);
    }

    // Final composition
    // Upsampled texture + original texture
    float4 FragComposition(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.texcoord;
        uint2 positionSS = uv * _ScreenSize.xy;
        const float dx = _InputTex_TexelSize.x * 1.5;

        float u0 = uv.x - dx;
        float u1 = uv.x;
        float u2 = uv.x + dx;

        float3 c0 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u0, uv.y)).rgb;
        float3 c1 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u1, uv.y)).rgb;
        float3 c2 = SAMPLE_TEXTURE2D(_InputTex, sampler_InputTex, float2(u2, uv.y)).rgb;
        float3 c3 = LOAD_TEXTURE2D_X(_CamColTex, positionSS).rgb;
        float3 cf = (c0 / 4 + c1 / 2 + c2 / 4) * _Color * _Intensity * 5;

        return float4(cf + c3, 1);

        // test 
        // return float4(c0, 1);
    }

    ENDHLSL

    SubShader
    {
        LOD 100
        ZWrite Off 
        Cull Off

        Pass
        {
            Name "Prefilter"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragPrefilter
            ENDHLSL
        }

        Pass
        {
            Name "Downsample"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDownsample
            ENDHLSL
        }

        Pass
        {
            Name "Upsample"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragUpsample
            ENDHLSL
        }

        Pass
        {
            Name "Composition"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposition
            ENDHLSL
        }
    }
}