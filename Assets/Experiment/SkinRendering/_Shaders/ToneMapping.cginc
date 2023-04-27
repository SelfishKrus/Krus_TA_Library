#ifndef ___TONEMAPPING_INCLUDED____
#define ___TONEMAPPING_INCLUDED____

#include "UnityCG.cginc"

float _Exposure;

float4 Tonemap(float3 rgb)
{
#if _TONEMAP_ON
    rgb *= _Exposure;
    rgb = max(0, rgb - 0.004);
    rgb = (rgb * (6.2 * rgb + 0.5)) / (rgb * (6.2 * rgb + 1.7) + 0.06);
    rgb = pow(rgb, 2.2);
#endif
    
    return float4(rgb, 1);
}


#endif