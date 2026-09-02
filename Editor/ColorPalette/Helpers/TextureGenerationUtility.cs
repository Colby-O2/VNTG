using UnityEngine;
using System.Collections.Generic;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    TextureGenerationUtility.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class TextureGenerationUtility
    {
        public static float GetAngleAt(Vector2 delta)
        {
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360.0f;
            return angle;
        }

        public static Color GetWheelColorAt(Vector2 delta, float saturation = 0f, float brightness = 1f, List<Color> palette = null)
        {
            float angle = GetAngleAt(delta);

            if (palette != null && palette.Count > 0)
            {
                float sliceSize = 360.0f / palette.Count;

                int colorIndex = Mathf.FloorToInt(angle / sliceSize);
                colorIndex = Mathf.Clamp(colorIndex, 0, palette.Count - 1);

                Color paletteColor = palette[colorIndex];
                paletteColor.a = 1.0f;

                return paletteColor;
            }

            return Color.HSVToRGB(angle / 360.0f, saturation, brightness);
        }

        private static Color GetSliceColor(Vector2 delta, float distFromCenter, List<Color> palette)
        {
            if (palette == null || palette.Count <= 1)
            {
                return GetWheelColorAt(delta, 0.6f, 0.7f, palette);
            }

            float angle = GetAngleAt(delta);

            float sliceSize = 360.0f / palette.Count;
            float angularPixelWidth = (1.0f / (distFromCenter + 0.001f)) * Mathf.Rad2Deg;

            float remainder = angle % sliceSize;
            float distToEdge = Mathf.Min(remainder, sliceSize - remainder);

            if (distToEdge < angularPixelWidth * 0.5f)
            {
                float t = Mathf.Clamp01((distToEdge + (angularPixelWidth * 0.5f)) / angularPixelWidth);

                int currentIndex = Mathf.FloorToInt(angle / sliceSize) % palette.Count;
                int neighborIndex = (remainder < sliceSize * 0.5f)
                    ? (currentIndex - 1 + palette.Count) % palette.Count
                    : (currentIndex + 1) % palette.Count;

                Color c1 = palette[neighborIndex];
                Color c2 = palette[currentIndex];
                c1.a = 1.0f;
                c2.a = 1.0f;

                return Color.Lerp(c1, c2, t);
            }

            return GetWheelColorAt(delta, 0.6f, 0.7f, palette);
        }

        public static void GenerateColorWheel(Color32[] buffer, int width, int height, float brightness = 1f)
        {
            Vector2 center = new Vector2(width, height) * 0.5f;
            float radius = (Mathf.Min(width, height) * 0.5f) - 1f;

            float aaWidth = 1.0f;
            float halfAA = aaWidth * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Vector2 delta = new Vector2(x, y) - center;
                    float dist = delta.magnitude;

                    if (dist > radius + halfAA)
                    {
                        buffer[index] = new Color32(0, 0, 0, 0);
                    }
                    else
                    {
                        Color sampledColor = GetWheelColorAt(delta, Mathf.Clamp01(dist / radius), brightness);

                        if (dist > radius - halfAA)
                        {
                            float alphaCoverage = Mathf.Clamp01((radius + halfAA - dist) / aaWidth);
                            sampledColor.a *= alphaCoverage;
                        }

                        buffer[index] = sampledColor;
                    }
                }
            }
        }

        public static Color SamplePreviewColorWheel
        (
            Vector2 delta, 
            float distFromCenter, 
            float wheelRadius, 
            float outlineSize, 
            Color outlineColor,
            Color fallbackColor,
            List<Color> palette = null
        )
        {
            float aaWidth = 1.0f;
            float innerEdge = wheelRadius;
            float outerEdge = wheelRadius + outlineSize;

            if (distFromCenter >= outerEdge + aaWidth) return fallbackColor;

            Color wheelColor = GetSliceColor(delta, distFromCenter, palette);

            float toOutline = Mathf.Clamp01((distFromCenter - (innerEdge - aaWidth * 0.5f)) / aaWidth);
            float toBackground = Mathf.Clamp01((distFromCenter - (outerEdge - aaWidth * 0.5f)) / aaWidth);

            Color mixedColor = Color.Lerp(wheelColor, outlineColor, toOutline);
            return Color.Lerp(mixedColor, fallbackColor, toBackground);
        }

        public static float ChamferedRectSDF(Vector2 p, Vector2 size, float radius, float cutSize)
        {
            float absX = Mathf.Abs(p.x);
            float absY = Mathf.Abs(p.y);

            float dx = Mathf.Max(absX - size.x + radius, 0.0f);
            float dy = Mathf.Max(absY - size.y + radius, 0.0f);
            float boxSDF = Mathf.Sqrt(dx * dx + dy * dy) - radius;

            float cutPlane = (p.x - (size.x - cutSize)) + (p.y - (size.y - cutSize)) - cutSize;

            return Mathf.Max(boxSDF, cutPlane);
        }
    }
}