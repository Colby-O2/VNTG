using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteImporter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{

    [ScriptedImporter(1, new[] { "hex", "gpl", "ase", "pal" })]
    public class PaletteImporter : ScriptedImporter
    {
        [Header("Palette Conversion Table Generation Settings")]
        [SerializeField] private PaletteDistanceMetric distanceMetric = PaletteDistanceMetric.CIE76;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string ext = Path.GetExtension(ctx.assetPath).ToLower().Replace(".", "");
            List<Color> parsedColors = new List<Color>();

            try
            {
                if (ext == "hex")
                {
                    parsedColors = PaletteReader.ParseHex(ctx.assetPath);
                }
                else if (ext == "gpl")
                {
                    parsedColors = PaletteReader.ParseGpl(ctx.assetPath);
                }
                else if (ext == "ase")
                {
                    parsedColors = PaletteReader.ParseAse(ctx.assetPath);
                }
                else if (ext == "pal")
                {
                    parsedColors = PaletteReader.ParsePal(ctx.assetPath);
                }
            }
            catch (Exception e)
            {
                ctx.LogImportError($"Failed to parse palette file: {ctx.assetPath}. Error: {e.Message}");
            }

            PaletteAsset paletteAsset = ScriptableObject.CreateInstance<PaletteAsset>();
            paletteAsset.colors = parsedColors;
            paletteAsset.distanceMetric = distanceMetric;

            Texture3D embeddedLUT = LUTBaker.BakePaletteTo3DLUT(parsedColors, distanceMetric);

            if (embeddedLUT != null)
            {
                embeddedLUT.name = "Baked_3DLUT";
                paletteAsset.bakedLUT3D = embeddedLUT;

                ctx.AddObjectToAsset("lut texture", embeddedLUT);
            }

            ctx.AddObjectToAsset("main obj", paletteAsset);
            ctx.SetMainObject(paletteAsset);
        }
    }
}