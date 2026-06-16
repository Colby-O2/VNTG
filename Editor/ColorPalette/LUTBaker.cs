using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    LUTBaker.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class LUTBaker
    {
        private const int LUT_DIM = 32;

        public static Texture3D BakePaletteTo3DLUT(List<Color> palette, PaletteDistanceMetric metric)
        {
            if (palette == null || palette.Count == 0) return null;

            Texture3D lut = new Texture3D(LUT_DIM, LUT_DIM, LUT_DIM, TextureFormat.RGBA32, false);
            lut.filterMode = FilterMode.Point;
            lut.wrapMode = TextureWrapMode.Clamp;

            Color32[] lutColors = new Color32[LUT_DIM * LUT_DIM * LUT_DIM];
            Color32[] palette32 = new Color32[palette.Count];
            for (int i = 0; i < palette.Count; i++) palette32[i] = palette[i];

            Color paletteCol = new Color();
            Color target = new Color();

            for (int r = 0; r < LUT_DIM; r++)
            {
                for (int g = 0; g < LUT_DIM; g++)
                {
                    for (int b = 0; b < LUT_DIM; b++)
                    {
                        target.r = (float)r / (LUT_DIM - 1);
                        target.g = (float)g / (LUT_DIM - 1);
                        target.b = (float)b / (LUT_DIM - 1);

                        int bestIndex = 0;
                        float minDistance = float.MaxValue;

                        for (int i = 0; i < palette32.Length; i++)
                        {
                            paletteCol.r = palette32[i].r / 255f;
                            paletteCol.g = palette32[i].g / 255f;
                            paletteCol.b = palette32[i].b / 255f;

                            float distSq = float.MaxValue;

                            switch (metric)
                            {
                                case PaletteDistanceMetric.EuclideanRGB: distSq = ColorDistanceCalculator.GetRBGDistance(target, paletteCol); break;
                                case PaletteDistanceMetric.Redmean: distSq = ColorDistanceCalculator.GetRedmeanDistance(target, paletteCol); break;
                                case PaletteDistanceMetric.Luminance: distSq = ColorDistanceCalculator.GetLuminanceDistance(target, paletteCol); break;
                                case PaletteDistanceMetric.Hue: distSq = ColorDistanceCalculator.GetHueDistance(target, paletteCol); break;
                                case PaletteDistanceMetric.CIE76: distSq = ColorDistanceCalculator.GetCIE76(target, paletteCol); break;
                                case PaletteDistanceMetric.CIE94: distSq = ColorDistanceCalculator.GetCIE94(target, paletteCol); break;
                                case PaletteDistanceMetric.CMC1984: distSq = ColorDistanceCalculator.GetCMClc(target, paletteCol); break;
                                case PaletteDistanceMetric.CIEDE2000: distSq = ColorDistanceCalculator.GetCIEDE2000(target, paletteCol); break;
                                case PaletteDistanceMetric.HyAB: distSq = ColorDistanceCalculator.GetHybridABDistance(target, paletteCol); break;
                                case PaletteDistanceMetric.BT2124_ITP: distSq = ColorDistanceCalculator.GetDeltaEITP(target, paletteCol); break;
                            }

                            if (distSq < minDistance)
                            {
                                minDistance = distSq;
                                bestIndex = i;
                            }
                        }

                        int index = r + (g * LUT_DIM) + (b * LUT_DIM * LUT_DIM);
                        lutColors[index] = palette32[bestIndex];
                    }
                }
            }

            lut.SetPixels32(lutColors);
            lut.Apply();
            return lut;
        }
    }
}
