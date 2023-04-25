#ifndef ___SS_UTILS____
#define ___SS_UTILS____

float2 Remap(float2 original_value, float2 original_min,
    float2 original_max, float2 new_min, float2 new_max)
{
    return new_min + (((original_value - original_min) /
        (original_max - original_min)) * (new_max - new_min));

}

float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
{
    return normalize(cross(normal, tangent.xyz)
        * binormalSign * unity_WorldTransformParams.w);
}

float3 UnpackSNormal(float4 packednormal, float bumpScale)
{
    #if defined(UNITY_NO_DXT5nm)
	    return packednormal.xyz * 2 - 1;
    #else
        half3 normal;
        normal.xy = (packednormal.wy * 2 - 1);
        normal.xy *= bumpScale;
        normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
        return normal;
    #endif
}

#endif