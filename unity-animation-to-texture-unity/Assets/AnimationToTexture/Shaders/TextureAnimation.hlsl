#ifndef TEXTURE_ANIMATION_INCLUDED
    #define TEXTURE_ANIMATION_INCLUDED

    TEXTURE2D(_AnimTexPos0);
    SAMPLER(sampler_AnimTexPos0);
    TEXTURE2D(_AnimTexNorm0);
    SAMPLER(sampler_AnimTexNorm0);
    TEXTURE2D(_AnimTexPos1);
    SAMPLER(sampler_AnimTexPos1);
    TEXTURE2D(_AnimTexNorm1);
    SAMPLER(sampler_AnimTexNorm1);
    TEXTURE2D(_AnimTexPos2);
    SAMPLER(sampler_AnimTexPos2);
    TEXTURE2D(_AnimTexNorm2);
    SAMPLER(sampler_AnimTexNorm2);
    TEXTURE2D(_AnimTexPos3);
    SAMPLER(sampler_AnimTexPos3);
    TEXTURE2D(_AnimTexNorm3);
    SAMPLER(sampler_AnimTexNorm3);

    float _TexelSize;
    float4 _AnimParams; // x: index, y: time, z: animLengthInv, w: isLooping (0 or 1)
    float _RandomSeed; // animParams2?

    #define TEXTURE_ANIMATION_OUTPUT(positionOS, normalOS, vertexID) TextureAnimation(positionOS, normalOS, vertexID);

    void TextureAnimation(out float3 positionOS, out float3 normalOS, uint vertexID)
    {
        float time = (_Time.y - _AnimParams.y) * _AnimParams.z;
        if(_AnimParams.w > 0.5) // looping
        {
            time += _RandomSeed * 172.827412; // randomize
        }
        // Animation Texture wrap mode should be set to Repeat if looping and Clamp if not looping
        // Otherwise, 'time' needs to be repeated or clamped manually

        // x: vertexID, y: time
        float2 uv = float2((float(vertexID) + 0.5) * _TexelSize, time);

        if(_AnimParams.x == 1){
            positionOS = SAMPLE_TEXTURE2D_LOD(_AnimTexPos1, sampler_AnimTexPos1, uv, 0).xyz;
            normalOS = SAMPLE_TEXTURE2D_LOD(_AnimTexNorm1, sampler_AnimTexNorm1, uv, 0).xyz;
        }
        else if(_AnimParams.x == 2){
            positionOS = SAMPLE_TEXTURE2D_LOD(_AnimTexPos2, sampler_AnimTexPos2, uv, 0).xyz;
            normalOS = SAMPLE_TEXTURE2D_LOD(_AnimTexNorm2, sampler_AnimTexNorm2, uv, 0).xyz;
        }
        else if(_AnimParams.x == 3){
            positionOS = SAMPLE_TEXTURE2D_LOD(_AnimTexPos3, sampler_AnimTexPos3, uv, 0).xyz;
            normalOS = SAMPLE_TEXTURE2D_LOD(_AnimTexNorm3, sampler_AnimTexNorm3, uv, 0).xyz;
        }
        else { // param.x == 0
            positionOS = SAMPLE_TEXTURE2D_LOD(_AnimTexPos0, sampler_AnimTexPos0, uv, 0).xyz;
            normalOS = SAMPLE_TEXTURE2D_LOD(_AnimTexNorm0, sampler_AnimTexNorm0, uv, 0).xyz;
        }
    }

    // For ShaderGraph
    void TextureAnimation_float(uint vertexID, out float3 positionOS, out float3 normalOS)
    {
        TextureAnimation(positionOS, normalOS, vertexID);
    }
    void TextureAnimation_half(uint vertexID, out half3 positionOS, out half3 normalOS)
    {
        TextureAnimation(positionOS, normalOS, vertexID);
    }

#endif