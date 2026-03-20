//-----------------------------------------------------------------------
// Original Author:      Codrin-Mihail
// Modifications Author: Colby-O
// File:                 PSXLighting_URP.hlsl
// 
// This file is a derivative work based on code from Codrin-Mihail
// https://github.com/Kodrin/URP-PSX/blob/master/URP-PSX/Assets/Shaders/HLSL/CustomLighting.hlsl
//-----------------------------------------------------------------------
// Key Modifications:
// - Integrated texel snapping logic (Adapted from GreatestBear)
// - Added support for Unlit shaders
// - Refactored MainLight to handle lighting calculations internally
// - Implemented URP LIGHT_LOOP macros for additional lights, supporting Forward+ and Deferred Rendering 
//   in addition to Forward Rendering which was originally supported
// - Fixed shadows for additional lights and added custom shadow tinting and distance cutoffs
// - Added light cookie support
// - Refactored overall code structure
//-------------------------------------------------------------------------
// The below license applies to the original portions of the code by Codrin-Mihail.
//-------------------------------------------------------------------------
// MIT License

// Copyright (c) 2020 Codrin-Mihail

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//-------------------------------------------------------------------------
// Modifications by Colby-O are released under The Unlicense
//-------------------------------------------------------------------------
// This is free and unencumbered software released into the public domain.

// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.

// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//-------------------------------------------------------------------------

#ifndef PSX_LIGHTING_INCLUDED
#define PSX_LIGHTING_INCLUDED

#if SHADERPASS == SHADERPASS_UNLIT

#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
#pragma multi_compile _ _ADDITIONAL_LIGHTS

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

#endif

/** 
    Code is adapted from GreatestBear

    References:
        [1] https://discussions.unity.com/t/the-quest-for-efficient-per-texel-lighting/700574
*/
void SnapPositionToTexel_float(
    float3 WorldPosition,
    float2 UV,
    float4 TexelSize,

    out float3 SnappedWorldPosition

)
{
    if (TexelSize.z < 0.001 || TexelSize.w < 0.001)
    {
        SnappedWorldPosition = WorldPosition;
        return;
    }
    
    float2 originalUV = UV;
    float2 centerUV = floor(originalUV * (TexelSize.zw)) / TexelSize.zw + (TexelSize.xy / 2.0);
    float2 dUV = (centerUV - originalUV);

    float2 dUVdS = ddx(originalUV);
    float2 dUVdT = ddy(originalUV);

    float2x2 dSTdUV = float2x2(dUVdT[1], -dUVdT[0], -dUVdS[1], dUVdS[0]) * (1.0f / (dUVdS[0] * dUVdT[1] - dUVdT[0] * dUVdS[1]));

    float2 dST = mul(dSTdUV, dUV);

    float3 dXYZdS = ddx(WorldPosition);
    float3 dXYZdT = ddy(WorldPosition);

    float3 dXYZ = dXYZdS * dST[0] + dXYZdT * dST[1];
    dXYZ = clamp(dXYZ, -1, 1);

    SnappedWorldPosition = (WorldPosition + dXYZ);
}

/** 
    Code is adapted from GreatestBear

    References:
        [1] https://discussions.unity.com/t/the-quest-for-efficient-per-texel-lighting/700574
*/
void SnapPositionToTexel_half(
    half3 WorldPosition,
    half2 UV,
    half4 TexelSize,

    out half3 SnappedWorldPosition

)
{
    if (TexelSize.z < 0.001 || TexelSize.w < 0.001)
    {
        SnappedWorldPosition = WorldPosition;
        return;
    }
    
    half2 originalUV = UV;
    half2 centerUV = floor(originalUV * (TexelSize.zw)) / TexelSize.zw + (TexelSize.xy / 2.0);
    half2 dUV = (centerUV - originalUV);

    half2 dUVdS = ddx(originalUV);
    half2 dUVdT = ddy(originalUV);

    half2x2 dSTdUV = half2x2(dUVdT[1], -dUVdT[0], -dUVdS[1], dUVdS[0]) * (1.0f / (dUVdS[0] * dUVdT[1] - dUVdT[0] * dUVdS[1]));

    half2 dST = mul(dSTdUV, dUV);

    half3 dXYZdS = ddx(WorldPosition);
    half3 dXYZdT = ddy(WorldPosition);

    half3 dXYZ = dXYZdS * dST[0] + dXYZdT * dST[1];
    dXYZ = clamp(dXYZ, -1, 1);

    SnappedWorldPosition = (WorldPosition + dXYZ);
}

void MainLight_float(
    float3 WorldPosition,
    float3 WorldNormal,
    float3 WorldView,
    float3 SpecColor,
    float Smoothness,
    bool EnableShadows,
    float3 ShadowTint,
    float ShadowDistCutoff,
    float ShadowOffset,

    out float3 Diffuse, 
    out float3 Specular
)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    
#if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(WorldPos);
    half4 shadowCoord = ComputeScreenPos(clipPos);
#else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPosition);
#endif
    
    // TODO FIX: Shadows are kinda borken (with severe acene) due to the vertex jitter. 
    // ShadowCorrd.z + ShadowOffset is a hack and casue peter paning artifacts.
    // For now set the offset to a small number and max out the shadow bias 
    // in the renderer asset for shadows.
    shadowCoord.z += ShadowOffset;
    
    Light mainLight = GetMainLight(shadowCoord);
    
    float distToCamera = distance(_WorldSpaceCameraPos, WorldPosition);
    float shadowAtten = (EnableShadows && distToCamera < ShadowDistCutoff) ? mainLight.shadowAttenuation : 1.0;
    float3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * shadowAtten) + ShadowTint * (1.0 - shadowAtten);

    diffuseColor = LightingLambert(attenuatedLightColor, mainLight.direction, WorldNormal);
    specularColor = LightingSpecular(attenuatedLightColor, mainLight.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
    
#endif
    
    Diffuse = diffuseColor;
    Specular = specularColor;
}

void MainLight_half(
    half3 WorldPosition,
    half3 WorldNormal,
    half3 WorldView,
    half3 SpecColor,
    half Smoothness,
    bool EnableShadows,
    half3 ShadowTint,
    half ShadowDistCutoff,
    half ShadowOffset,

    out half3 Diffuse,
    out half3 Specular
)
{
    half3 diffuseColor = 0;
    half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    
#if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(WorldPos);
    half4 shadowCoord = ComputeScreenPos(clipPos);
#else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPosition);
#endif
    
    // TODO FIX: Shadows are kinda borken (with serve acene) due to the vertex jitter. 
    // ShadowCorrd.z + ShadowOffset is a hack and casue peter paning artifacts.
    // For now set the offset to a small number and max out the shadow bias 
    // in the renderer asset for shadows.
    shadowCoord.z += ShadowOffset;
    
    Light mainLight = GetMainLight(shadowCoord);
    
    half distToCamera = distance(_WorldSpaceCameraPos, WorldPosition);
    half shadowAtten = (EnableShadows && distToCamera < ShadowDistCutoff) ? mainLight.shadowAttenuation : 1.0;
    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * shadowAtten) + ShadowTint * (1.0 - shadowAtten);

    diffuseColor = LightingLambert(attenuatedLightColor, mainLight.direction, WorldNormal);
    specularColor = LightingSpecular(attenuatedLightColor, mainLight.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);
    
#endif
    
    Diffuse = diffuseColor;
    Specular = specularColor;
}

void AdditionalLights_float(
    float3 SpecColor, 
    float Smoothness, 
    float3 WorldPosition, 
    float3 WorldNormal, 
    float3 WorldView, 
    bool EnableShadows,
    float3 ShadowTint,
    float ShadowDistCutoff,

    out float3 Diffuse, 
    out float3 Specular
)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);

    uint pixelLightCount = GetAdditionalLightsCount();

    InputData inputData = (InputData) 0;
    inputData.positionWS = WorldPosition;
    
    float4 clipPos = TransformWorldToHClip(WorldPosition);
    float4 screenPos = ComputeScreenPos(clipPos);
    inputData.normalizedScreenSpaceUV = screenPos.xy / (screenPos.w + 0.00001);
    
    float distToCamera = distance(_WorldSpaceCameraPos, WorldPosition);
    
    LIGHT_LOOP_BEGIN(pixelLightCount)

    // TODO FIX: Shadows are kinda borken (with severe acene) due to the vertex jitter. 
    // ShadowCoord seems to do nothing for additional lights so to fix you'll need to max out the depth
    // bias in your renderer asset for now.
    half4 shadowCoord = CalculateShadowMask(inputData);
    
    Light light = GetAdditionalLight(lightIndex, WorldPosition, shadowCoord);
        
#ifdef _LIGHT_COOKIES
            light.color *= SampleAdditionalLightCookie(lightIndex, WorldPosition);
#endif
    
    float shadowAtten = (EnableShadows && distToCamera < ShadowDistCutoff) ? light.shadowAttenuation : 1.0;
    float3 attenuatedLightColor = light.color * (light.distanceAttenuation * shadowAtten) + ShadowTint * (1.0 - shadowAtten);
        
    diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
    specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
    
    LIGHT_LOOP_END
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

void AdditionalLights_half(
    half3 SpecColor,
    half Smoothness,
    half3 WorldPosition,
    half3 WorldNormal,
    half3 WorldView,
    bool EnableShadows,
    half3 ShadowTint,
    half ShadowDistCutoff,

    out half3 Diffuse,
    out half3 Specular
)
{
    half3 diffuseColor = 0;
    half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);

    uint pixelLightCount = GetAdditionalLightsCount();

    InputData inputData = (InputData) 0;
    inputData.positionWS = WorldPosition;
    
    half4 clipPos = TransformWorldToHClip(WorldPosition);
    half4 screenPos = ComputeScreenPos(clipPos);
    inputData.normalizedScreenSpaceUV = screenPos.xy / (screenPos.w + 0.00001);
    
    half distToCamera = distance(_WorldSpaceCameraPos, WorldPosition);
    
    LIGHT_LOOP_BEGIN(pixelLightCount)

    // TODO FIX: Shadows are kinda borken (with serve acene) due to the vertex jitter. 
    // ShadowCoord seems to do nothing for additional lights so to fix you'll need to max out the depth
    // bias in your renderer asset for now.
    half4 shadowCoord = CalculateShadowMask(inputData);
    Light light = GetAdditionalLight(lightIndex, WorldPosition, shadowCoord);
        
#ifdef _LIGHT_COOKIES
            light.color *= SampleAdditionalLightCookie(lightIndex, WorldPosition);
#endif

    half shadowAtten = (EnableShadows && distToCamera < ShadowDistCutoff) ? light.shadowAttenuation : 1.0;
    half3 attenuatedLightColor = light.color * (light.distanceAttenuation * shadowAtten) + ShadowTint * (1.0 - shadowAtten);
        
    diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
    specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);
    
    LIGHT_LOOP_END
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

#endif