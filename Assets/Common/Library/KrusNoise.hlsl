#ifndef KRUS_NOISE
#define KRUS_NOISE

inline float rand(half2 uv)
    {
        return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
    }

inline float rand(half3 uvw)
    {
        return frac(sin(dot(uvw,float3(12.9898,78.233,45.164)))*43758.5453123);
    }

#endif