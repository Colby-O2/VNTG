using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    Palette.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette
{
    [CreateAssetMenu(fileName = "New Palette", menuName = "VNTG/Color Palette")]
    public class Palette : ScriptableObject
    {
        public static readonly int LUT_DIM = 32;
        public static readonly int LUT_SIZE = LUT_DIM * LUT_DIM * LUT_DIM;

        public PaletteDistanceMetric distanceMetric = PaletteDistanceMetric.CIE76;

        [Header("Palette Colors")]
        public List<Color> colors = new List<Color>();

        [HideInInspector] public Texture3D LUT;
        [HideInInspector] public Color32[] RawLUT;

        private void OnDestroy()
        {
            DestroyTexture(LUT);
        }

        public Color Sample(Color inputColor)
        {
            if (RawLUT == null || RawLUT.Length != LUT_SIZE)
            {
                Debug.LogWarning($"Palette '{name}': Can't sample palette because it doesn't ahve a vaild LUT!");
                return inputColor;
            }

            inputColor.r = Mathf.Clamp01(inputColor.r);
            inputColor.g = Mathf.Clamp01(inputColor.g);
            inputColor.b = Mathf.Clamp01(inputColor.b);

            int r = Mathf.Clamp(Mathf.FloorToInt(inputColor.r * (LUT_DIM - 1) + 0.5f), 0, LUT_DIM - 1);
            int g = Mathf.Clamp(Mathf.FloorToInt(inputColor.g * (LUT_DIM - 1) + 0.5f), 0, LUT_DIM - 1);
            int b = Mathf.Clamp(Mathf.FloorToInt(inputColor.b * (LUT_DIM - 1) + 0.5f), 0, LUT_DIM - 1);

            int index = r + (g * LUT_DIM) + (b * LUT_DIM * LUT_DIM);

            return RawLUT[index];
        }

        public void UpdateLUT(Texture3D lut, Color32[] rawLUT)
        {
            if (LUT != null && lut != LUT)
            {

#if UNITY_EDITOR
                if (!Application.isPlaying && AssetDatabase.Contains(LUT))
                {
                    AssetDatabase.RemoveObjectFromAsset(LUT);
                }
#endif

                DestroyTexture(LUT);
            }

            LUT = lut;
            RawLUT = rawLUT;
        }

        public bool HasLUT()
        {
            return LUT != null && RawLUT != null && RawLUT.Length != 0;
        }

        private void DestroyTexture(Texture3D tex)
        {
            if (tex == null) return;

            if (Application.isPlaying)
            {
                Destroy(tex);
            }
            else
            {
                DestroyImmediate(tex, true);
            }
        }

    }
}
