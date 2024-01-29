#ifndef _K_TRIPLANAR_PROJECTION
#define _K_TRIPLANAR_PROJECTION

    // Mapping UV for different axes
    struct TriplanarUV
    {
        float2 x, y, z;
    };

    // Mapping UV based on 3d position
    TriplanarUV GetTriplanarUV (float3 pos, float3 normal, float2 ST)
    {
        TriplanarUV triUV;
        triUV.x = pos.zy * ST.x + ST.y;
        triUV.y = pos.xz * ST.x + ST.y;
        triUV.z = pos.xy * ST.x + ST.y;

        // Fix mirror problems
        if (normal.x < 0) triUV.x.x = -triUV.x.x;
        if (normal.y < 0) triUV.y.x = -triUV.y.x;
        if (normal.z >= 0) triUV.z.x = -triUV.z.x;

        // Remove repetition
        triUV.x.y += 0.5;
        triUV.z.x += 0.5;

        return triUV;
    }

    // Get weight based on surface orientation (normal)
    float3 GetTriplanarWeights (float3 normal, float blendOffset)
    {   
        float3 triW = abs(normal);
        triW = max(triW - blendOffset, 0.001);
        // triW.x + triW.y + triW.z = 1
        return triW / (triW.x + triW.y + triW.z);
    }

    
    // Main //
    float3 TriplanarSampling(Texture2D map, SamplerState sampler_map, float3 normal, float3 pos, float2 ST, float blendOffset)
    {
        TriplanarUV triUV = GetTriplanarUV(pos, normal, ST);
        float3 triW = GetTriplanarWeights(normal, blendOffset);
        
        float3 mapX = map.Sample(sampler_map, triUV.x);
        float3 mapY = map.Sample(sampler_map, triUV.y);
        float3 mapZ = map.Sample(sampler_map, triUV.z);

        return mapX * triW.x + mapY * triW.y + mapZ * triW.z;
    }

#endif 