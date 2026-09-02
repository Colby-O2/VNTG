//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    N64Sampler.hlsl
//-----------------------------------------------------------------------

#ifndef COLBYO_VNTG_N64_SAMPLER_INCLUDED
#define COLBYO_VNTG_N64_SAMPLER_INCLUDED

void N64Sampler_float(UnityTexture2D Texture, UnitySamplerState Sampler, float4 TexelSize, float2 UV, out float4 Out)
{
    uint width, height;
    Texture.tex.GetDimensions(width, height);
    TexelSize = float4(1.0 / width, 1.0 / height, width, height);
    
    float2 texels = UV * TexelSize.zw;
    
    //TexelSize.xy *= -1.0;
    texels = texels - 0.5;
    
    float2 fracTexel = frac(texels);
    float3 blend = float3(
        abs(fracTexel.x + fracTexel.y - 1),
        min(abs(fracTexel.xx - float2(0.0, 1.0)), abs(fracTexel.yy - float2(1.0, 0.0)))
    );
    
    float2 uvA = (floor(texels + fracTexel.yx) + 0.5) * TexelSize.xy;
    float2 uvB = (floor(texels) + float2(1.5, 0.5)) * TexelSize.xy;
    float2 uvC = (floor(texels) + float2(0.5, 1.5)) * TexelSize.xy;
    
    float4 A = SAMPLE_TEXTURE2D_LOD(Texture.tex, Sampler.samplerstate, uvA, 0.0);
    float4 B = SAMPLE_TEXTURE2D_LOD(Texture.tex, Sampler.samplerstate, uvB, 0.0);
    float4 C = SAMPLE_TEXTURE2D_LOD(Texture.tex, Sampler.samplerstate, uvC, 0.0);
    
    Out = A * blend.x + B * blend.y + C * blend.z;
}

void N64Sampler_half(UnityTexture2D Texture, UnitySamplerState Sampler, half4 TexelSize, half2 UV, out half4 Out)
{
    half width, height;
    Texture.tex.GetDimensions(width, height);
    TexelSize = half4(1.0 / width, 1.0 / height, width, height);
    
    half2 texels = UV * TexelSize.zw;
    
    //TexelSize.xy *= -1.0;
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

void DirToCubeFaceUV_float(float3 dir, out int faceIndex, out float2 uv)
{
    float3 absDir = abs(dir);
    float maxAxis;

    if (absDir.x >= absDir.y && absDir.x >= absDir.z)
    {
        faceIndex = (dir.x > 0.0) ? 0 : 1;
        maxAxis = absDir.x;
        uv = float2((dir.x > 0.0) ? -dir.z : dir.z, -dir.y);
    }
    else if (absDir.y >= absDir.x && absDir.y >= absDir.z)
    {
        faceIndex = (dir.y > 0.0) ? 2 : 3;
        maxAxis = absDir.y;
        uv = float2(dir.x, (dir.y > 0.0) ? dir.z : -dir.z);
    }
    else
    {
        faceIndex = (dir.z > 0.0) ? 4 : 5;
        maxAxis = absDir.z;
        uv = float2((dir.z > 0.0) ? dir.x : -dir.x, -dir.y);
    }

    uv = 0.5 * (uv / maxAxis + 1.0);
}

void DirToCubeFaceUV_half(half3 dir, out int faceIndex, out half2 uv)
{
    half3 absDir = abs(dir);
    half maxAxis;

    if (absDir.x >= absDir.y && absDir.x >= absDir.z)
    {
        faceIndex = (dir.x > 0.0) ? 0 : 1;
        maxAxis = absDir.x;
        uv = half2((dir.x > 0.0) ? -dir.z : dir.z, -dir.y);
    }
    else if (absDir.y >= absDir.x && absDir.y >= absDir.z)
    {
        faceIndex = (dir.y > 0.0) ? 2 : 3;
        maxAxis = absDir.y;
        uv = half2(dir.x, (dir.y > 0.0) ? dir.z : -dir.z);
    }
    else
    {
        faceIndex = (dir.z > 0.0) ? 4 : 5;
        maxAxis = absDir.z;
        uv = half2((dir.z > 0.0) ? dir.x : -dir.x, -dir.y);
    }

    uv = 0.5 * (uv / maxAxis + 1.0);
}

float3 CubeFaceUVToDir_float(int faceIndex, float2 uv)
{
    float2 uc = uv * 2.0 - 1.0;
    float3 dir = float3(0, 0, 0);

    switch (faceIndex)
    {
        case 0:
            dir = float3(1.0, -uc.y, -uc.x);
            break;
        case 1:
            dir = float3(-1.0, -uc.y, uc.x);
            break;
        case 2:
            dir = float3(uc.x, 1.0, uc.y);
            break;
        case 3:
            dir = float3(uc.x, -1.0, -uc.y);
            break;
        case 4:
            dir = float3(uc.x, -uc.y, 1.0);
            break;
        case 5:
            dir = float3(-uc.x, -uc.y, -1.0);
            break;
    }

    return normalize(dir);
}

half3 CubeFaceUVToDir_half(int faceIndex, half2 uv)
{
    half2 uc = uv * 2.0 - 1.0;
    half3 dir = half3(0, 0, 0);

    switch (faceIndex)
    {
        case 0:
            dir = half3(1.0, -uc.y, -uc.x);
            break;
        case 1:
            dir = half3(-1.0, -uc.y, uc.x);
            break;
        case 2:
            dir = half3(uc.x, 1.0, uc.y);
            break;
        case 3:
            dir = half3(uc.x, -1.0, -uc.y);
            break;
        case 4:
            dir = half3(uc.x, -uc.y, 1.0);
            break;
        case 5:
            dir = half3(-uc.x, -uc.y, -1.0);
            break;
    }

    return normalize(dir);
}

void N64SamplerCube_float(
    UnityTextureCube Cube, 
    UnitySamplerState Sampler, 
    float3 Dir, 

    out float4 Out
)
{
    uint width, height;
    Cube.tex.GetDimensions(width, height);
    
    int faceIndex;
    float2 UV;
    DirToCubeFaceUV_float(Dir, faceIndex, UV);

    float4 TexelSize = float4(1.0 / width, 1.0 / height, width, height);
    float2 texels = UV * TexelSize.zw - 0.5;

    float2 fracTexel = frac(texels);
    float3 blend = float3(
        abs(fracTexel.x + fracTexel.y - 1.0),
        min(abs(fracTexel.xx - float2(0.0, 1.0)), abs(fracTexel.yy - float2(1.0, 0.0)))
    );

    float2 uvA = (floor(texels + fracTexel.yx) + 0.5) * TexelSize.xy;
    float2 uvB = (floor(texels) + float2(1.5, 0.5)) * TexelSize.xy;
    float2 uvC = (floor(texels) + float2(0.5, 1.5)) * TexelSize.xy;

    float3 dirA = CubeFaceUVToDir_float(faceIndex, uvA);
    float3 dirB = CubeFaceUVToDir_float(faceIndex, uvB);
    float3 dirC = CubeFaceUVToDir_float(faceIndex, uvC);

    float4 A = SAMPLE_TEXTURECUBE_LOD(Cube.tex, Sampler.samplerstate, dirA, 0.0);
    float4 B = SAMPLE_TEXTURECUBE_LOD(Cube.tex, Sampler.samplerstate, dirB, 0.0);
    float4 C = SAMPLE_TEXTURECUBE_LOD(Cube.tex, Sampler.samplerstate, dirC, 0.0);

    Out = A * blend.x + B * blend.y + C * blend.z;
}

void N64SamplerCube_half(
    UnityTextureCube Cube,
    UnitySamplerState Sampler,
    half3 Dir,

    out half4 Out
)
{
    uint width, height;
    Cube.tex.GetDimensions(width, height);
    
    int faceIndex;
    half2 UV;
    DirToCubeFaceUV_half(Dir, faceIndex, UV);

    half4 TexelSize = half4(1.0 / width, 1.0 / height, width, height);
    half2 texels = UV * TexelSize.zw - 0.5;

    half2 fracTexel = frac(texels);
    half3 blend = half3(
        abs(fracTexel.x + fracTexel.y - 1.0),
        min(abs(fracTexel.xx - half2(0.0, 1.0)), abs(fracTexel.yy - half2(1.0, 0.0)))
    );

    half2 uvA = (floor(texels + fracTexel.yx) + 0.5) * TexelSize.xy;
    half2 uvB = (floor(texels) + half2(1.5, 0.5)) * TexelSize.xy;
    half2 uvC = (floor(texels) + half2(0.5, 1.5)) * TexelSize.xy;

    half3 dirA = CubeFaceUVToDir_half(faceIndex, uvA);
    half3 dirB = CubeFaceUVToDir_half(faceIndex, uvB);
    half3 dirC = CubeFaceUVToDir_half(faceIndex, uvC);

    half4 A = SAMPLE_TEXTURECUBE_LOD(Cube.tex, Sampler.samplerstate, dirA, 0.0);
    half4 B = SAMPLE_TEXTURECUBE_LOD(Cube.tex, Sampler.samplerstate, dirB, 0.0);
    half4 C = SAMPLE_TEXTURECUBE_LOD(Cube.tex, Sampler.samplerstate, dirC, 0.0);

    Out = A * blend.x + B * blend.y + C * blend.z;
}

#endif