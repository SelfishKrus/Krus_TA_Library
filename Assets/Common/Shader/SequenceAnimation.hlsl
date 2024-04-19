#ifndef SEQUENCE_ANIMATION_INCLUDED
#define SEQUENCE_ANIMATION_INCLUDED

    void SequenceAnimationUV_float(float uNum, float vNum, float time, float speed, float2 uv, out float2 uv_sequence)
    {
        float intTime = floor(time * speed);
        float row = floor(intTime / uNum);
        float column = intTime - row * uNum;

        uv_sequence = uv + float2(column, -row);
        uv_sequence.x /= uNum;
        uv_sequence.y /= vNum;
    }

#endif 