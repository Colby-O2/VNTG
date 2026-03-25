//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    HSVConversion.hlsl
//-----------------------------------------------------------------------

#ifndef HSV_CONVERSION_INCLUDED
#define HSV_CONVERSION_INCLUDED

void RgbToHsv_float(float3 RGB, out float3 HSV)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    
    float4 p = lerp(float4(RGB.bg, K.wz), float4(RGB.gb, K.xy), step(RGB.b, RGB.g));
    float4 q = lerp(float4(p.xyw, RGB.r), float4(RGB.r, p.yzx), step(p.x, RGB.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    
    HSV = float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

void RgbToHsv_half(half3 RGB, out half3 HSV)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    
    half4 p = lerp(half4(RGB.bg, K.wz), half4(RGB.gb, K.xy), step(RGB.b, RGB.g));
    half4 q = lerp(half4(p.xyw, RGB.r), half4(RGB.r, p.yzx), step(p.x, RGB.r));

    half d = q.x - min(q.w, q.y);
    half e = 1e-4;
    
    HSV = half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

void HsvToRgb_float(float3 HSV, out float3 RGB)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    
    float3 p = abs(frac(HSV.xxx + K.xyz) * 6.0 - K.www);
    
    RGB = HSV.z * lerp(K.xxx, saturate(p - K.xxx), HSV.y);
}

void HsvToRgb_half(half3 HSV, out half3 RGB)
{
    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    
    half3 p = abs(frac(HSV.xxx + K.xyz) * 6.0 - K.www);
    
    RGB = HSV.z * lerp(K.xxx, saturate(p - K.xxx), HSV.y);
}

#endif