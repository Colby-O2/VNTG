//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    HSVConversion.hlsl
//-----------------------------------------------------------------------

#ifndef HSV_CONVERSION_INCLUDED
#define HSV_CONVERSION_INCLUDED

void RgbToHsv_float(float3 RBG, out float3 HSV)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    
    float4 p = lerp(float4(RBG.bg, K.wz), float4(RBG.gb, K.xy), step(RBG.b, RBG.g));
    float4 q = lerp(float4(p.xyw, RBG.r), float4(RBG.r, p.yzx), step(p.x, RBG.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    
    HSV = float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

void RgbToHsv_half(half3 RBG, out half3 HSV)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    
    half4 p = lerp(half4(RBG.bg, K.wz), half4(RBG.gb, K.xy), step(RBG.b, RBG.g));
    half4 q = lerp(half4(p.xyw, RBG.r), half4(RBG.r, p.yzx), step(p.x, RBG.r));

    half d = q.x - min(q.w, q.y);
    half e = 1.0e-10;
    
    HSV = half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

void HsvToRgb_float(float3 HSV, out float3 RBG)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    
    float3 p = abs(frac(HSV.xxx + K.xyz) * 6.0 - K.www);
    
    RBG = HSV.z * lerp(K.xxx, saturate(p - K.xxx), HSV.y);
}

void HsvToRgb_half(half3 HSV, out half3 RBG)
{
    half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    
    half3 p = abs(frac(HSV.xxx + K.xyz) * 6.0 - K.www);
    
    RBG = HSV.z * lerp(K.xxx, saturate(p - K.xxx), HSV.y);
}

#endif