using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaintNetTextPalettePostprocessor.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class PaintNetTextPalettePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (!assetPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) continue;

                try
                {
                    using (StreamReader reader = new StreamReader(assetPath))
                    {
                        string firstLine = reader.ReadLine();
                        if (firstLine != null && firstLine.Trim().StartsWith(";paint.net Palette File"))
                        {
                            ConvertTxtToPaletteAsset(assetPath);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"VNTG Postprocessor failed checking text asset: {assetPath}. Error: {e.Message}");
                }
            }
        }

        private static void ConvertTxtToPaletteAsset(string assetPath)
        {
            List<Color> parsedColors = PaletteReader.ParsePaintNet(assetPath);

            PaletteAsset paletteAsset = ScriptableObject.CreateInstance<PaletteAsset>();
            paletteAsset.colors = parsedColors;

            paletteAsset.distanceMetric = PaletteDistanceMetric.CIE76;

            Texture3D embeddedLUT = LUTBaker.BakePaletteTo3DLUT(parsedColors, paletteAsset.distanceMetric);
            if (embeddedLUT != null)
            {
                embeddedLUT.name = "Baked_3DLUT";
                paletteAsset.bakedLUT3D = embeddedLUT;
            }

            string newAssetPath = Path.ChangeExtension(assetPath, ".asset");
            AssetDatabase.CreateAsset(paletteAsset, newAssetPath);

            if (embeddedLUT != null)
            {
                AssetDatabase.AddObjectToAsset(embeddedLUT, paletteAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.DeleteAsset(assetPath);
        }
    }
}