#ifndef SJSJ_BTDF_INCLUDED
#define SJSJ_BRDF_INCLUDED

// Fast Transmission Approximation - Colin Barré-Brisebois/ Marc Bouchard - GDC 2011 // 
// for directional light only
half3 EvaluateTransmission_CM(
    half3 l,
    half3 n,
    half _Distortion,
    half3 v, 
    half thickness,
    half3 _TransmissionCol,
    half3 lightCol
) {
    half3 h_LplusN = normalize(l + n * _Distortion);
    half3 hv_BTDF = max(saturate(dot(v, -h_LplusN)), 0.00001);
    half3 I = hv_BTDF * thickness;
    half3 transCol_DL = _TransmissionCol * I * lightCol;

    return transCol_DL;
}

#endif
