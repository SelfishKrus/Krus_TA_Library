#ifndef _TONEMAPPING_INCLUDED
#define _TONEMAPPING_INCLUDED

half3 Tonemapping_Filmic(float3 rgb, half _Exposure)
{
    rgb *= _Exposure;
    rgb = max(0, rgb - 0.004);
    rgb = (rgb * (6.2 * rgb + 0.5)) / (rgb * (6.2 * rgb + 1.7) + 0.06);
    rgb = pow(rgb, 2.2);
    
    return rgb;
}

#endif