//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    N64Sampler.hlsl
//-----------------------------------------------------------------------

#ifndef N64_Sampler_INCLUDED
#define N64_Sampler_INCLUDED

void N64Sampler_float(UnityTexture2D Texture, UnitySamplerState Sampler, float4 TexelSize, float2 UV, out float4 Out)
{
    float2 texels = UV * TexelSize.zw;
    
    texels = texels - 0.5;
    
    float2 fracTexel = frac(texels);
    float3 blend = float3(
        abs(fracTexel.x + fracTexel.y - 1), 
        min(abs(fracTexel.xx - float2(0.0, 1.0)), abs(fracTexel.yy - float2(1.0, 0.0)))
    );
    
    float2 uvA = (floor(texels + fracTexel.yx) + 0.5) * TexelSize.xy;
    float2 uvB = (floor(texels) + float2(1.5, 0.5)) * TexelSize.xy;
    float2 uvC = (floor(texels) + float2(0.5, 1.5)) * TexelSize.xy;
    
    float4 A = SAMPLE_TEXTURE2D_LOD(Texture, Sampler, uvA, 0.0);
    float4 B = SAMPLE_TEXTURE2D_LOD(Texture, Sampler, uvB, 0.0);
    float4 C = SAMPLE_TEXTURE2D_LOD(Texture, Sampler, uvC, 0.0);
    
    Out = A * blend.x + B * blend.y + C * blend.z;
}

void N64Sampler_half(UnityTexture2D Texture, UnitySamplerState Sampler, half4 TexelSize, half2 UV, out half4 Out)
{
    half2 texels = UV * TexelSize.zw;
    
    TexelSize.xy *= -1.0;
    texels = texels - 0.5;
    
    half2 fracTexel = frac(texels);
    half3 blend = half3(
        abs(fracTexel.x + fracTexel.y - 1),
        min(abs(fracTexel.xx - half2(0.0, 1.0)), abs(fracTexel.yy - half2(1.0, 0.0)))
    );
    
    half2 uvA = (floor(texels + fracTexel.yx) + 0.5) * TexelSize.xy;
    half2 uvB = (floor(texels) + half2(1.5, 0.5)) * TexelSize.xy;
    half2 uvC = (floor(texels) + half2(0.5, 1.5)) * TexelSize.xy;
    
    half4 A = SAMPLE_TEXTURE2D_LOD(Texture, Sampler, uvA, 0.0);
    half4 B = SAMPLE_TEXTURE2D_LOD(Texture, Sampler, uvB, 0.0);
    half4 C = SAMPLE_TEXTURE2D_LOD(Texture, Sampler, uvC, 0.0);
    
    Out = A * blend.x + B * blend.y + C * blend.z;
}

#endif