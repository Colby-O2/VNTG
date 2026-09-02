using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    ColorExtension.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette
{
    public static class VNTGColorSpace
    {
        public static Color Clamp(this Color color, float min, float max)
        {
            color.r = Mathf.Clamp(color.r, min, max);
            color.g = Mathf.Clamp(color.g, min, max);
            color.b = Mathf.Clamp(color.b, min, max);
            return color;
        }

        public static Color Clamp01(this Color color)
        {
            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);
            return color;
        }

        public static float GetChromaAtLightness(float L)
        {
            float t = Mathf.Abs(L - 50f) / 50f;
            return 128f * (1f - t);
        }

        public static float GetOklabChromaAtLightness(float L)
        {
            float t = Mathf.Abs(L - 0.5f) / 0.5f;
            return 0.4f * (1f - t);
        }

        public static void HSVToHSL(float _h, float _s, float _v, out float h, out float s, out float l)
        {
            h = _h;

            l = _v * (1f - _s * 0.5f);

            if (l == 0f || l == 1f)
            {
                s = 0f;
            }
            else
            {
                s = (_v - l) / Mathf.Min(l, 1f - l);
            }
        }

        public static void HSLToHSV(float _h, float _s, float _l, out float h, out float s, out float v)
        {
            h = _h;

            v = _l + _s * Mathf.Min(_l, 1f - _l);

            if (v == 0f)
            {
                s = 0f;
            }
            else
            {
                s = 2f * (1f - _l / v);
            }
        }

        public static void RGBToHSL(this Color color, out float h, out float s, out float l)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;

            float max = Mathf.Max(r, Mathf.Max(g, b));
            float min = Mathf.Min(r, Mathf.Min(g, b));
            float delta = max - min;

            l = (max + min) * 0.5f;

            if (delta == 0f)
            {
                h = 0.0f;
                s = 0.0f;
            }
            else
            {
                s = l < 0.5f ? delta / (max + min) : delta / (2f - max - min);

                if (max == r)
                {
                    h = (g - b) / delta + (g < b ? 6.0f : 0.0f);
                }
                else if (max == g)
                {
                    h = (b - r) / delta + 2.0f;
                }
                else
                {
                    h = (r - g) / delta + 4.0f;
                }

                h /= 6.0f;
            }
        }

        public static Color HslToRGB(float _h, float _s, float _l)
        {
            float r, g, b;

            if (_s == 0.0f)
            {
                r = g = b = _l;
            }
            else
            {
                float q = _l < 0.5f ? _l * (1f + _s) : _l + _s - _l * _s;
                float p = 2f * _l - q;

                r = HueToRGB(p, q, _h + 1.0f / 3.0f);
                g = HueToRGB(p, q, _h);
                b = HueToRGB(p, q, _h - 1.0f / 3.0f);
            }

            return new Color(r, g, b, 1.0f);
        }

        private static float HueToRGB(float p, float q, float t)
        {
            if (t < 0.0f) t += 1.0f;
            if (t > 1.0f) t -= 1.0f;
            if (t < 1.0f / 6.0f) return p + (q - p) * 6.0f * t;
            if (t < 1.0f / 2.0f) return q;
            if (t < 2.0f / 3.0f) return p + (q - p) * (2.0f / 3.0f - t) * 6.0f;
            return p;
        }

        public static void RGBToLab(this Color color, out float L, out float a, out float b)
        {
            Color linearColor = color.linear;

            float _r = linearColor.r;
            float _g = linearColor.g;
            float _b = linearColor.b;

            float x = _r * 0.412456f + _g * 0.3575561f + _b * 0.1804375f;
            float y = _r * 0.2126729f + _g * 0.7151522f + _b * 0.0721750f;
            float z = _r * 0.0193339f + _g * 0.1191920f + _b * 0.9503041f;

            x /= 0.95047f; 
            y /= 1.00000f; 
            z /= 1.08883f;

            x = (x > 0.008856f) ? Mathf.Pow(x, 1f / 3f) : (7.787f * x) + (16f / 116f);
            y = (y > 0.008856f) ? Mathf.Pow(y, 1f / 3f) : (7.787f * y) + (16f / 116f);
            z = (z > 0.008856f) ? Mathf.Pow(z, 1f / 3f) : (7.787f * z) + (16f / 116f);

            L = (116f * y) - 16f;
            a = 500f * (x - y);
            b = 200f * (y - z);
        }

        public static Color LabToRGB(float _L, float _a, float _b)
        {
            float y = (_L + 16f) / 116f;
            float x = (_a / 500f) + y;
            float z = y - (_b / 200f);

            float x3 = Mathf.Pow(x, 3f);
            float y3 = Mathf.Pow(y, 3f);
            float z3 = Mathf.Pow(z, 3f);

            x = (x3 > 0.008856f) ? x3 : (x - 16f / 116f) / 7.787f;
            y = (y3 > 0.008856f) ? y3 : (y - 16f / 116f) / 7.787f;
            z = (z3 > 0.008856f) ? z3 : (z - 16f / 116f) / 7.787f;

            x *= 0.95047f;
            y *= 1.00000f;
            z *= 1.08883f;

            float r = x * 3.2404542f + y * -1.5371385f + z * -0.4985314f;
            float g = x * -0.9692660f + y * 1.8760108f + z * 0.0415560f;
            float b = x * 0.0556434f + y * -0.2040259f + z * 1.0572252f;

            Color linearColor = new Color(r, g, b);
            return linearColor.gamma;
        }

        public static void RGBToOklab(this Color color, out float L, out float a, out float b)
        {
            Color linearColor = color.linear;

            float l_cone = linearColor.r * 0.4122214708f + linearColor.g * 0.5363325363f + linearColor.b * 0.0514459929f;
            float m_cone = linearColor.r * 0.2119034982f + linearColor.g * 0.6806995451f + linearColor.b * 0.1073969566f;
            float s_cone = linearColor.r * 0.0883024619f + linearColor.g * 0.2817188376f + linearColor.b * 0.6299787005f;

            float l_ = Mathf.Pow(l_cone, 1f / 3f);
            float m_ = Mathf.Pow(m_cone, 1f / 3f);
            float s_ = Mathf.Pow(s_cone, 1f / 3f);

            L = l_ * 0.2104542553f + m_ * 0.7936177850f - s_ * 0.0040720468f;
            a = l_ * 1.9779984951f - m_ * 2.4285922050f + s_ * 0.4505937099f;
            b = l_ * 0.0259040371f + m_ * 0.7827717662f - s_ * 0.8086757660f;
        }

        public static Color OklabToRGB(float _L, float _a, float _b)
        {
            float l_ = _L + 0.3963377774f * _a + 0.2158037573f * _b;
            float m_ = _L - 0.1055613458f * _a - 0.0638541728f * _b;
            float s_ = _L - 0.0894841775f * _a - 1.2914855480f * _b;

            float l_cone = l_ * l_ * l_;
            float m_cone = m_ * m_ * m_;
            float s_cone = s_ * s_ * s_;

            float r = l_cone * +4.0767416621f + m_cone * -3.3077115913f + s_cone * +0.2309699292f;
            float g = l_cone * -1.2684380046f + m_cone * +2.6097574011f + s_cone * -0.3413190470f;
            float b = l_cone * -0.0041960863f + m_cone * -0.7034186147f + s_cone * +1.7076147010f;

            Color linearColor = new Color(r, g, b);
            return linearColor.gamma;
        }

        public static void RGBToITP(this Color color, out float i, out float t, out float p)
        {
            Color linearColor = color.linear;

            float r = linearColor.r;
            float g = linearColor.g;
            float b = linearColor.b;

            float l = r * 0.3592f + g * 0.5134f + b * 0.1274f;
            float m = r * 0.1344f + g * 0.7453f + b * 0.1203f;
            float s = r * 0.0619f + g * 0.1916f + b * 0.7465f;

            l = ApplyPQ(l);
            m = ApplyPQ(m);
            s = ApplyPQ(s);

            float I = 0.5f * l + 0.5f * m;
            float Ct = 1.6137f * l - 3.3234f * m + 1.7097f * s;
            float Cp = 4.3781f * l - 4.2455f * m - 0.1325f * s;

            i = I;
            t = 0.5f * Ct;
            p = Cp;
        }

        public static Color ITPToRGB(float _i, float _t, float _p)
        {
            float Ct = _t / 0.5f;
            float Cp = _p;
            float I = _i;

            float l = I * 1.0f + Ct * 0.008609037f + Cp * 0.11102962f;
            float m = I * 1.0f + Ct * -0.008609037f + Cp * -0.11102962f;
            float s = I * 1.0f + Ct * 0.5600313f + Cp * -0.32062714f;

            l = InverseApplyPQ(l);
            m = InverseApplyPQ(m);
            s = InverseApplyPQ(s);

            float r = l * 3.4366066f + m * -2.506452f + s * 0.06984555f;
            float g = l * -0.7913296f + m * 1.9779913f + s * -0.18666175f;
            float b = l * -0.0259497f + m * -0.2434954f + s * 1.269445f;

            Color linearColor = new Color(r, g, b);
            return linearColor.gamma;
        }

        private static float ApplyPQ(float x)
        {
            if (x <= 0f) return 0f;

            const float m1 = 2610f / 16384f;
            const float m2 = 2523f / 32f;
            const float c1 = 3424f / 4064f;
            const float c2 = 2413f / 4096f;
            const float c3 = 2392f / 4096f;

            float xPowM1 = Mathf.Pow(x, m1);
            float num = c1 + c2 * xPowM1;
            float den = 1f + c3 * xPowM1;

            return Mathf.Pow(num / den, m2);
        }

        private static float InverseApplyPQ(float y)
        {
            if (y <= 0f) return 0f;

            const float m1 = 2610f / 16384f;
            const float m2 = 2523f / 32f;
            const float c1 = 3424f / 4064f;
            const float c2 = 2413f / 4096f;
            const float c3 = 2392f / 4096f;

            float yPowInvM2 = Mathf.Pow(y, 1f / m2);
            float num = Mathf.Max(0f, yPowInvM2 - c1);
            float den = c2 - c3 * yPowInvM2;

            return Mathf.Pow(num / den, 1f / m1);
        }
    }
}
