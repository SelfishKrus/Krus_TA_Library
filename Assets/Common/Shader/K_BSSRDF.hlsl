#ifndef K_BSSRDF_INCLUDED
#define K_BSSRDF_INCLUDED

    half3 EvaluatePISSS(
        half3 normalWS,
        half3 normalBlurredWS,
        half3 lightDirWS,
        half curvature,
        half3 lightCol,
        half3 baseCol,
        sampler2D _SSLUT_Curvature,
        half2 _CurvatureBias_uv,
        half k_d
    ) {
        // SS from curvature by LUT
        half nl_blurredUnclamped = dot(normalBlurredWS, lightDirWS);
        half curvatureScaled = curvature * _CurvatureBias_uv.x + _CurvatureBias_uv.y;
        half2 uvCurvature = half2(nl_blurredUnclamped * 0.5 + 0.5, curvatureScaled);
        half3 diffCol_curvature = tex2D(_SSLUT_Curvature, uvCurvature).rgb;

        half3 Li = lightCol * k_d;
        half3 diffCol = diffCol_curvature * baseCol * Li ;

        return diffCol;

    }

#endif