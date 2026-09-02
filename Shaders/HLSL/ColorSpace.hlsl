#ifndef COLBYO_VNTG_COLOR_SPACE_INCLUDED
#define COLBYO_VNTG_COLOR_SPACE_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float3 VNTG_SRGBToLinear(float3 c)
{
    return SRGBToLinear(c);
}

float3 VNTG_LinearToSRGB(float3 c)
{
    return LinearToSRGB(c);
}

float GetChromaAtLightness(float L)
{
    float t = abs(L - 50.0) / 50.0;
    return 128.0 * (1.0 - t);
}

float GetOklabChromaAtLightness(float L)
{
    float t = abs(L - 0.5) / 0.5;
    return 0.4 * (1.0 - t);
}

float3 HSVToHSL(float3 hsv)
{
    float h = hsv.x;
    float s = hsv.y;
    float v = hsv.z;
    float l = v * (1.0 - s / 2.0);
    float minVal = min(l, 1.0 - l);
    float newS = (minVal < 0.0001) ? 0.0 : (v - l) / minVal;
    return float3(h, newS, l);
}

float3 HSLToHSV(float3 hsl)
{
    float h = hsl.x;
    float s = hsl.y;
    float l = hsl.z;
    float v = l + s * min(l, 1.0 - l);
    float newS = (v < 0.0001) ? 0.0 : 2.0 * (1.0 - l / v);
    return float3(h, newS, v);
}

float3 HSVToRGB(float3 hsv)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
    return hsv.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), hsv.y);
}

float3 RGBToHSV(float3 rgb)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);  
    float4 p = lerp(float4(rgb.bg, K.wz), float4(rgb.gb, K.xy), step(rgb.b, rgb.g));
    float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSLToRGB(float3 hsl)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(hsl.xxx + K.xyz) * 6.0 - K.www);
    float3 rgb = lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), hsl.y);
    return hsl.z + hsl.y * (rgb - 0.5) * (1.0 - abs(2.0 * hsl.z - 1.0));
}

float3 RGBToHSL(float3 rgb)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(rgb.bg, K.wz), float4(rgb.gb, K.xy), step(rgb.b, rgb.g));
    float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));

    float maxVal = q.x;
    float minVal = min(q.w, q.y);
    float d = maxVal - minVal;
    float e = 1.0e-10;
    
    float h = abs(q.z + (q.w - q.y) / (6.0 * d + e));
    float l = (maxVal + minVal) * 0.5;
    float s = d / (1.0 - abs(2.0 * l - 1.0) + e);
    
    return float3(h, s, l);
}

float3 RGBToLab(float3 rgb)
{
    float3 linearColor = VNTG_SRGBToLinear(rgb);

    float x = linearColor.r * 0.4124564 + linearColor.g * 0.3575761 + linearColor.b * 0.1804375;
    float y = linearColor.r * 0.2126729 + linearColor.g * 0.7151522 + linearColor.b * 0.0721750;
    float z = linearColor.r * 0.0193339 + linearColor.g * 0.1191920 + linearColor.b * 0.9503041;

    x /= 0.95047;
    y /= 1.00000;
    z /= 1.08883;

    float3 v;
    v.x = (x > 0.008856) ? pow(x, 1.0 / 3.0) : (7.787 * x) + (16.0 / 116.0);
    v.y = (y > 0.008856) ? pow(y, 1.0 / 3.0) : (7.787 * y) + (16.0 / 116.0);
    v.z = (z > 0.008856) ? pow(z, 1.0 / 3.0) : (7.787 * z) + (16.0 / 116.0);

    float L = (116.0 * v.y) - 16.0;
    float a = 500.0 * (v.x - v.y);
    float b = 200.0 * (v.y - v.z);

    return float3(L, a, b);
}

float3 LabToRGB(float L, float a, float b)
{
    float y = (L + 16.0) / 116.0;
    float x = (a / 500.0) + y;
    float z = y - (b / 200.0);

    float x3 = x * x * x;
    float y3 = y * y * y;
    float z3 = z * z * z;

    x = (x3 > 0.008856) ? x3 : (x - 16.0 / 116.0) / 7.787;
    y = (y3 > 0.008856) ? y3 : (y - 16.0 / 116.0) / 7.787;
    z = (z3 > 0.008856) ? z3 : (z - 16.0 / 116.0) / 7.787;

    x *= 0.95047;
    y *= 1.00000;
    z *= 1.08883;

    float r = x * 3.2404542 + y * -1.5371385 + z * -0.4985314;
    float g = x * -0.9692660 + y * 1.8760108 + z * 0.0415560;
    float b_val = x * 0.0556434 + y * -0.2040259 + z * 1.0572252;

    float3 rgb = float3(r, g, b_val);

    return VNTG_LinearToSRGB(rgb);;
}

float3 RGBToOklab(float3 rgb)
{
    float3 linearColor = VNTG_SRGBToLinear(rgb);

    float l_cone = linearColor.r * 0.4122214708 + linearColor.g * 0.5363325363 + linearColor.b * 0.0514459929;
    float m_cone = linearColor.r * 0.2119034982 + linearColor.g * 0.6806995451 + linearColor.b * 0.1073969566;
    float s_cone = linearColor.r * 0.0883024619 + linearColor.g * 0.2817188376 + linearColor.b * 0.6299787005;

    float l_ = pow(max(0.0, l_cone), 1.0 / 3.0);
    float m_ = pow(max(0.0, m_cone), 1.0 / 3.0);
    float s_ = pow(max(0.0, s_cone), 1.0 / 3.0);

    float L = l_ * 0.2104542553 + m_ * 0.7936177850 - s_ * 0.0040720468;
    float a = l_ * 1.9779984951 - m_ * 2.4285922050 + s_ * 0.4505937099;
    float b = l_ * 0.0259040371 + m_ * 0.7827717662 - s_ * 0.8086757660;

    return float3(L, a, b);
}

float3 OklabToRGB(float L, float a, float b)
{
    float l_ = L + 0.3963377774 * a + 0.2158037573 * b;
    float m_ = L - 0.1055613458 * a - 0.0638541728 * b;
    float s_ = L - 0.0894841775 * a - 1.2914855480 * b;

    float l_cone = l_ * l_ * l_;
    float m_cone = m_ * m_ * m_;
    float s_cone = s_ * s_ * s_;

    float r = l_cone * 4.0767416621 + m_cone * -3.3077115913 + s_cone * 0.2309699292;
    float g = l_cone * -1.2684380046 + m_cone * 2.6097574011 + s_cone * -0.3413190470;
    float b_val = l_cone * -0.0041960863 + m_cone * -0.7034186147 + s_cone * 1.7076147010;

    float3 rgb = float3(r, g, b_val);

    return VNTG_LinearToSRGB(rgb);
}

float ApplyPQ(float x)
{
    if (x <= 0.0)
        return 0.0;

    const float m1 = 2610.0 / 16384.0;
    const float m2 = 2523.0 / 32.0;
    const float c1 = 3424.0 / 4064.0;
    const float c2 = 2413.0 / 4096.0;
    const float c3 = 2392.0 / 4096.0;

    float xPowM1 = pow(x, m1);
    float num = c1 + c2 * xPowM1;
    float den = 1.0 + c3 * xPowM1;

    return pow(num / den, m2);
}

float InverseApplyPQ(float y)
{
    if (y <= 0.0)
        return 0.0;

    const float m1 = 2610.0 / 16384.0;
    const float m2 = 2523.0 / 32.0;
    const float c1 = 3424.0 / 4064.0;
    const float c2 = 2413.0 / 4096.0;
    const float c3 = 2392.0 / 4096.0;

    float yPowInvM2 = pow(y, 1.0 / m2);
    float num = max(0.0, yPowInvM2 - c1);
    float den = c2 - c3 * yPowInvM2;

    return pow(num / den, 1.0 / m1);
}

float3 RGBToITP(float3 rgb)
{
    float3 linearColor = VNTG_SRGBToLinear(rgb);

    float l = linearColor.r * 0.3592 + linearColor.g * 0.5134 + linearColor.b * 0.1274;
    float m = linearColor.r * 0.1344 + linearColor.g * 0.7453 + linearColor.b * 0.1203;
    float s = linearColor.r * 0.0619 + linearColor.g * 0.1916 + linearColor.b * 0.7465;

    l = ApplyPQ(l);
    m = ApplyPQ(m);
    s = ApplyPQ(s);

    float I = 0.5 * l + 0.5 * m;
    float Ct = 1.6137 * l - 3.3234 * m + 1.7097 * s;
    float Cp = 4.3781 * l - 4.2455 * m - 0.1325 * s;

    return float3(I, 0.5 * Ct, Cp);
}

float3 ITPToRGB(float3 itp)
{
    float I = itp.x;
    float Ct = itp.y / 0.5;
    float Cp = itp.z;

    float l = I * 1.0 + Ct * 0.008609037 + Cp * 0.11102962;
    float m = I * 1.0 + Ct * -0.008609037 + Cp * -0.11102962;
    float s = I * 1.0 + Ct * 0.5600313 + Cp * -0.32062714;

    l = InverseApplyPQ(l);
    m = InverseApplyPQ(m);
    s = InverseApplyPQ(s);

    float r = l * 3.4366066 + m * -2.506452 + s * 0.06984555;
    float g = l * -0.7913296 + m * 1.9779913 + s * -0.18666175;
    float b = l * -0.0259497 + m * -0.2434954 + s * 1.269445;

    float3 rgb = float3(r, g, b);
    return VNTG_LinearToSRGB(rgb);
}

#endif