using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    TextureToPaletteConverter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class TextureToPaletteConverter
    {
        [MenuItem("Assets/Create/VNTG/Convert Texture to Palette Asset", false, 20)]
        private static void ConvertSelectedTextureToPalette()
        {
            Texture2D sourceTex = Selection.activeObject as Texture2D;
            if (sourceTex == null)
            {
                EditorUtility.DisplayDialog("Conversion Failed", "Please select a valid Texture2D asset first.", "OK");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(sourceTex);

            Texture2D readableTex = sourceTex;
            bool wasUnreadable = !sourceTex.isReadable;
            if (wasUnreadable)
            {
                RenderTexture tmp = RenderTexture.GetTemporary(sourceTex.width, sourceTex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
                Graphics.Blit(sourceTex, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;

                readableTex = new Texture2D(sourceTex.width, sourceTex.height);
                readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                readableTex.Apply();

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);
            }

            int width = readableTex.width;
            int height = readableTex.height;
            Color32[] rawPixels = readableTex.GetPixels32();

            List<Color> paletteColors = new List<Color>();
            HashSet<Color32> encounteredColors = new HashSet<Color32>();

            int targetY = height / 2;
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = targetY * width + x;
                Color32 c = rawPixels[pixelIndex];

                if (c.a == 0) continue;

                if (!encounteredColors.Contains(c))
                {
                    encounteredColors.Add(c);
                    paletteColors.Add(c);
                }
            }

            if (wasUnreadable)
            {
                UnityEngine.Object.DestroyImmediate(readableTex);
            }

            if (paletteColors.Count == 0)
            {
                EditorUtility.DisplayDialog("Conversion Failed", "No valid colors were found on the horizontal centerline of this texture.", "OK");
                return;
            }

            PaletteAsset paletteAsset = ScriptableObject.CreateInstance<PaletteAsset>();
            paletteAsset.colors = paletteColors;
            paletteAsset.distanceMetric = PaletteDistanceMetric.CIE76;

            Texture3D embeddedLUT = LUTBaker.BakePaletteTo3DLUT(paletteColors, paletteAsset.distanceMetric);
            if (embeddedLUT != null)
            {
                embeddedLUT.name = "Baked_3DLUT";
                paletteAsset.bakedLUT3D = embeddedLUT;
            }

            string directory = Path.GetDirectoryName(assetPath);
            string filename = Path.GetFileNameWithoutExtension(assetPath);
            string targetAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, $"{filename}_Palette.asset"));

            AssetDatabase.CreateAsset(paletteAsset, targetAssetPath);

            if (embeddedLUT != null)
            {
                AssetDatabase.AddObjectToAsset(embeddedLUT, paletteAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(paletteAsset);
            Selection.activeObject = paletteAsset;
        }

        [MenuItem("Assets/Create/VNTG/Convert Texture to Palette Asset", true)]
        private static bool ConvertSelectedTextureToPaletteValidate()
        {
            return Selection.activeObject is Texture2D;
        }
    }
}
