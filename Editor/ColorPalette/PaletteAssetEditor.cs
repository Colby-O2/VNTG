using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteAssetEditor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    [CustomEditor(typeof(PaletteAsset))]
    public class PaletteAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PaletteAsset palette = (PaletteAsset)target;
            string assetPath = AssetDatabase.GetAssetPath(palette);

            string ext = Path.GetExtension(assetPath).ToLower().Replace(".", "");

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            bool isScripted = importer is ScriptedImporter;

            if (AssetDatabase.IsSubAsset(palette) || isScripted || ext != "asset")
            {
                DrawDefaultInspector();
                return;
            }

            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            if (EditorGUI.EndChangeCheck())
            {
                RebakeAssetLUT(palette);
            }
        }

        private void RebakeAssetLUT(PaletteAsset palette)
        {
            if (palette == null) return;

            Texture3D newLut = LUTBaker.BakePaletteTo3DLUT(palette.colors, palette.distanceMetric);
            if (newLut == null) return;

            newLut.name = "Baked_3DLUT";

            if (palette.bakedLUT3D != null)
            {
                Undo.DestroyObjectImmediate(palette.bakedLUT3D);
            }

            palette.UpdateBakedLUT(newLut);
            AssetDatabase.AddObjectToAsset(newLut, palette);

            EditorUtility.SetDirty(palette);
            AssetDatabase.SaveAssets();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            PaletteAsset palette = (PaletteAsset)target;
            if (palette == null) return null;

            Color fileCardColor = new Color(0.2f, 0.2f, 0.2f, 1f);

            Texture2D previewTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color[] clearPixels = new Color[width * height];
            for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
            previewTex.SetPixels(clearPixels);

            float centerX = width / 2f;
            float centerY = height / 2f;
            float innerWheelRadius = width * 0.28f;

            float cardHalfWidth = width * 0.38f;
            float cardHalfHeight = height * 0.42f;
            float cornerRadius = width * 0.06f;

            float foldSize = width * 0.22f;
            float foldX = cardHalfWidth - foldSize;
            float foldY = cardHalfHeight - foldSize;

            List<Color> paletteColors = palette.colors;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distFromCenter = Mathf.Sqrt(dx * dx + dy * dy);

                    float sdfValue = RoundedRectWithCutSDF(dx, dy, cardHalfWidth, cardHalfHeight, cornerRadius, foldSize);

                    if (sdfValue <= 0)
                    {
                        Color pixelColor = fileCardColor;

                        if (sdfValue > -1.5f)
                        {
                            pixelColor = new Color(0.35f, 0.35f, 0.35f, 1f);
                        }

                        if (dx > foldX && dy > foldY)
                        {
                            float foldLine = (dx - foldX) + (dy - foldY);
                            if (foldLine > foldSize)
                            {
                                pixelColor = new Color(0.75f, 0.75f, 0.75f, 1f);

                                if (Mathf.Abs(foldLine - foldSize) < 1.5f)
                                {
                                    pixelColor = new Color(0.45f, 0.45f, 0.45f, 1f);
                                }
                            }
                        }

                        if (distFromCenter <= innerWheelRadius)
                        {
                            float wheelEdgeAlpha = Mathf.Clamp01(innerWheelRadius - distFromCenter);

                            Color targetWheelColor;
                            float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                            if (angle < 0) angle += 360f;

                            if (paletteColors != null && paletteColors.Count > 0)
                            {
                                float sliceSize = 360f / paletteColors.Count;
                                int colorIndex = Mathf.FloorToInt(angle / sliceSize);
                                colorIndex = Mathf.Clamp(colorIndex, 0, paletteColors.Count - 1);

                                targetWheelColor = paletteColors[colorIndex];
                            }
                            else
                            {
                                targetWheelColor = Color.HSVToRGB(angle / 360f, 0.6f, 0.7f);
                            }

                            targetWheelColor.a = 1f;
                            pixelColor = Color.Lerp(pixelColor, targetWheelColor, wheelEdgeAlpha);
                        }

                        previewTex.SetPixel(x, y, pixelColor);
                    }
                }
            }

            previewTex.Apply();
            return previewTex;
        }

        private float RoundedRectWithCutSDF(float x, float y, float sizeX, float sizeY, float radius, float cutSize)
        {
            float dx = Mathf.Max(Mathf.Abs(x) - sizeX + radius, 0);
            float dy = Mathf.Max(Mathf.Abs(y) - sizeY + radius, 0);
            float boxSDF = Mathf.Sqrt(dx * dx + dy * dy) - radius;

            float cutPlane = (x - (sizeX - cutSize)) + (y - (sizeY - cutSize)) - cutSize;

            return Mathf.Max(boxSDF, cutPlane);
        }
    }
}
