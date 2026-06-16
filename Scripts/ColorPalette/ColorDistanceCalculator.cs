using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    ColorDistanceCalculator.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette
{
    public static class ColorDistanceCalculator
    {
        public static Vector3 ToLab(Color color)
        {
            Color linearColor = color.linear;

            float r = linearColor.r;
            float g = linearColor.g;
            float b = linearColor.b;

            float x = r * 0.412456f + g * 0.3575561f + b * 0.1804375f;
            float y = r * 0.2126729f + g * 0.7151522f + b * 0.0721750f;
            float z = r * 0.0193339f + g * 0.1191920f + b * 0.9503041f;

            x /= 0.95047f; y /= 1.00000f; z /= 1.08883f;

            x = (x > 0.008856f) ? Mathf.Pow(x, 1f / 3f) : (7.787f * x) + (16f / 116f);
            y = (y > 0.008856f) ? Mathf.Pow(y, 1f / 3f) : (7.787f * y) + (16f / 116f);
            z = (z > 0.008856f) ? Mathf.Pow(z, 1f / 3f) : (7.787f * z) + (16f / 116f);

            return new Vector3((116f * y) - 16f, 500f * (x - y), 200f * (y - z));
        }

        public static Vector3 ToITP(Color color)
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

            return new Vector3(I, 0.5f * Ct, Cp);
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

        public static float GetRBGDistance(Color color1, Color color2)
        {
            float deltaR = color1.r - color2.r; 
            float deltaG = color1.g - color2.g;
            float deltaB = color1.b - color2.b;

            return deltaR * deltaR + deltaG * deltaG + deltaB * deltaB;
        }

        public static float GetRedmeanDistance(Color color1, Color color2)
        {
            float r1 = color1.r * 255f;
            float g1 = color1.g * 255f;
            float b1 = color1.b * 255f;

            float r2 = color2.r * 255f;
            float g2 = color2.g * 255f;
            float b2 = color2.b * 255f;

            float deltaR = r1 - r2;
            float deltaG = g1 - g2;
            float deltaB = b1 - b2;

            float barR = 0.5f * (r1 + r2);

            return (2f + barR / 256f) * deltaR * deltaR + 4f * deltaG * deltaG + (2f + (255f - barR) / 256f) * deltaB * deltaB;
        }

        public static float GetLuminance(Color color)
        {
            return (0.2126f * color.r) + (0.7152f * color.g) + (0.0722f * color.b);
        }

        public static float GetLuminanceDistance(Color color1, Color color2)
        {
            float deltaLum = GetLuminance(color1) - GetLuminance(color2);
            return deltaLum * deltaLum;
        }

        public static float GetHueDistance(Color color1, Color color2)
        {
            Vector3 lab1 = ToLab(color1);
            Vector3 lab2 = ToLab(color2);

            float h1 = Mathf.Atan2(lab1.z, lab1.y) * Mathf.Rad2Deg;
            if (h1 < 0f) h1 += 360f;

            float h2 = Mathf.Atan2(lab2.z, lab2.y) * Mathf.Rad2Deg;
            if (h2 < 0f) h2 += 360f;

            float deltaH = Mathf.Abs(h1 - h2);
            if (deltaH > 180f) deltaH = 360f - deltaH;

            return deltaH * deltaH;
        }

        public static float GetCIE76(Color color1, Color color2)
        {
            Vector3 lab1 = ToLab(color1);
            Vector3 lab2 = ToLab(color2);

            float deltaL = lab1.x - lab2.x;
            float deltaA = lab1.y - lab2.y;
            float deltaB = lab1.z - lab2.z;

            return deltaL * deltaL + deltaA * deltaA + deltaB * deltaB;
        }

        public static float GetCIE94(Color color1, Color color2)
        {
            Vector3 lab1 = ToLab(color1);
            Vector3 lab2 = ToLab(color2);

            float deltaL = lab1.x - lab2.x;

            float c1 = Mathf.Sqrt(lab1.y * lab1.y + lab1.z * lab1.z);
            float c2 = Mathf.Sqrt(lab2.y * lab2.y + lab2.z * lab2.z);
            float deltaC = c1 - c2;

            float deltaA = lab1.y - lab2.y;
            float deltaB = lab1.z - lab2.z;
            float deltaH2 = (deltaA * deltaA) + (deltaB * deltaB) - (deltaC * deltaC);

            float deltaH = (deltaH2 > 0) ? Mathf.Sqrt(deltaH2) : 0f;

            float kl = 1f;
            float kc = 1f;
            float kh = 1.0f;

            float k1 = 0.045f;
            float k2 = 0.015f;

            float sl = 1.0f;
            float sc = 1.0f + k1 * c1;
            float sh = 1.0f + k2 * c1;

            float vl = deltaL / (kl * sl);
            float vc = deltaC / (kc * sc);
            float vh = deltaH / (kh * sh);

            return (vl * vl) + (vc * vc) + (vh * vh);
        }

        public static float GetCMClc(Color color1, Color color2, float l = 1f, float c = 1f)
        {
            Vector3 lab1 = ToLab(color1);
            Vector3 lab2 = ToLab(color2);

            float deltaL = lab1.x - lab2.x;
            float deltaA = lab1.y - lab2.y;
            float deltaB = lab1.z - lab2.z;

            float c1 = Mathf.Sqrt(lab1.y * lab1.y + lab1.z * lab1.z);
            float c2 = Mathf.Sqrt(lab2.y * lab2.y + lab2.z * lab2.z);
            float deltaC = c1 - c2;

            float deltaH2 = (deltaA * deltaA) + (deltaB * deltaB) - (deltaC * deltaC);
            float deltaH = (deltaH2 > 0f) ? Mathf.Sqrt(deltaH2) : 0f;

            float h1 = Mathf.Atan2(lab1.z, lab1.y) * Mathf.Rad2Deg;
            if (h1 < 0f) h1 += 360f;

            float t;
            if (h1 >= 164f && h1 <= 345f)
            {
                t = 0.56f + Mathf.Abs(0.2f * Mathf.Cos((h1 + 168f) * Mathf.Deg2Rad));
            }
            else
            {
                t = 0.36f + Mathf.Abs(0.4f * Mathf.Cos((h1 + 35f) * Mathf.Deg2Rad));
            }

            float c1Pow4 = Mathf.Pow(c1, 4f);
            float f = Mathf.Sqrt(c1Pow4 / (c1Pow4 + 1900f));

            float sL;
            if (lab1.x < 16f)
            {
                sL = 0.511f;
            }
            else
            {
                sL = (0.040975f * lab1.x) / (1f + 0.01765f * lab1.x);
            }

            float sC = ((0.0638f * c1) / (1f + 0.0131f * c1)) + 0.638f;
            float sH = sC * (f * t + 1f - f);

            float vL = deltaL / (l * sL);
            float vC = deltaC / (c * sC);
            float vH = deltaH / sH;

            return Mathf.Sqrt((vL * vL) + (vC * vC) + (vH * vH));
        }

        public static float GetCIEDE2000(Color color1, Color color2)
        {
            Vector3 lab1 = ToLab(color1);
            Vector3 lab2 = ToLab(color2);

            float c1 = Mathf.Sqrt(lab1.y * lab1.y + lab1.z * lab1.z);
            float c2 = Mathf.Sqrt(lab2.y * lab2.y + lab2.z * lab2.z);
            float barC = (c1 + c2) / 2f;

            float barC7 = Mathf.Pow(barC, 7f);
            float g = 0.5f * (1.0f - Mathf.Sqrt(barC7 / (barC7 + 6103515625f)));

            float a1Prime = (1f + g) * lab1.y;
            float a2Prime = (1f + g) * lab2.y;

            float c1Prime = Mathf.Sqrt(a1Prime * a1Prime + lab1.z * lab1.z);
            float c2Prime = Mathf.Sqrt(a2Prime * a2Prime + lab2.z * lab2.z);

            float h1Prime = Mathf.Atan2(lab1.z, a1Prime) * Mathf.Rad2Deg;
            if (h1Prime < 0) h1Prime += 360f;
            if (Mathf.Abs(a1Prime) < 1e-5f && Mathf.Abs(lab1.z) < 1e-5f) h1Prime = 0f;

            float h2Prime = Mathf.Atan2(lab2.z, a2Prime) * Mathf.Rad2Deg;
            if (h2Prime < 0) h2Prime += 360f;
            if (Mathf.Abs(a2Prime) < 1e-5f && Mathf.Abs(lab2.z) < 1e-5f) h2Prime = 0f;

            float deltaLPrime = lab1.x - lab2.x;
            float deltaCPrime = c1Prime - c2Prime;

            float deltahPrime = 0f;
            if (c1Prime * c2Prime > 1e-5f)
            {
                if (Mathf.Abs(h1Prime - h2Prime) <= 180f)
                {
                    deltahPrime = h1Prime - h2Prime;
                }
                else if (h1Prime > h2Prime)
                {
                    deltahPrime = h1Prime - h2Prime - 360f;
                }
                else
                {
                    deltahPrime = h1Prime - h2Prime + 360f;
                }
            }

            float deltaHPrime = 2f * Mathf.Sqrt(c1Prime * c2Prime) * Mathf.Sin(deltahPrime * 0.5f * Mathf.Deg2Rad);

            float barLPrime = (lab1.x + lab2.x) / 2f;
            float barCPrime = (c1Prime + c2Prime) / 2f;

            float barhPrime = 0f;
            if (c1Prime * c2Prime > 1e-5f)
            {
                if (Mathf.Abs(h1Prime - h2Prime) <= 180f)
                {
                    barhPrime = (h1Prime + h2Prime) / 2f;
                }
                else if (h1Prime + h2Prime < 360f)
                {
                    barhPrime = (h1Prime + h2Prime + 360f) / 2f;
                }
                else
                {
                    barhPrime = (h1Prime + h2Prime - 360f) / 2f;
                }
            }
            else
            {
                barhPrime = h1Prime + h2Prime;
            }

            float t = 1f - 0.17f * Mathf.Cos((barhPrime - 30f) * Mathf.Deg2Rad)
                        + 0.24f * Mathf.Cos((2f * barhPrime) * Mathf.Deg2Rad)
                        + 0.32f * Mathf.Cos((3f * barhPrime + 6f) * Mathf.Deg2Rad)
                        - 0.20f * Mathf.Cos((4f * barhPrime - 63f) * Mathf.Deg2Rad);

            float deltaTheta = 30f * Mathf.Exp(-Mathf.Pow((barhPrime - 275f) / 25f, 2f));

            float barCPrime7 = Mathf.Pow(barCPrime, 7f);
            float rc = 2f * Mathf.Sqrt(barCPrime7 / (barCPrime7 + 6103515625f));
            float rt = -rc * Mathf.Sin(2f * deltaTheta * Mathf.Deg2Rad);

            float lMinus50Sq = Mathf.Pow(barLPrime - 50f, 2f);
            float sl = 1f + (0.015f * lMinus50Sq) / Mathf.Sqrt(20f + lMinus50Sq);
            float sc = 1f + 0.045f * barCPrime;
            float sh = 1f + 0.015f * barCPrime * t;

            float kl = 1f;
            float kc = 1f;
            float kh = 1f;

            float dLKlSl = deltaLPrime / (kl * sl);
            float dCKcSc = deltaCPrime / (kc * sc);
            float dHKhSh = deltaHPrime / (kh * sh);

            return Mathf.Sqrt(Mathf.Pow(dLKlSl, 2f) + Mathf.Pow(dCKcSc, 2f) + Mathf.Pow(dHKhSh, 2f) + rt * dCKcSc * dHKhSh);
        }

        public static float GetDeltaEITP(Color color1, Color color2)
        {
            Vector3 itp1 = ToITP(color1);
            Vector3 itp2 = ToITP(color2);

            float deltaI = itp1.x - itp2.x;
            float deltaT = itp1.y - itp2.y;
            float deltaP = itp1.z - itp2.z;

            return 720f * Mathf.Sqrt(deltaI * deltaI + deltaT * deltaT + deltaP * deltaP);
        }

        public static float GetHybridABDistance(Color color1, Color color2)
        {
            Vector3 lab1 = ToLab(color1);
            Vector3 lab2 = ToLab(color2);

            float deltaA = lab1.y - lab2.y;
            float deltaB = lab1.z - lab2.z;

            float chromaticDistanceSq = (deltaA * deltaA) + (deltaB * deltaB);

            float deltaL = Mathf.Abs(lab1.x - lab2.x);

            float hyAB = Mathf.Sqrt(chromaticDistanceSq) + deltaL;
            return hyAB * hyAB;
        }
    }
}