using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteAsset.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette
{
    [CreateAssetMenu(fileName = "New Palette", menuName = "VNTG/Palette Asset")]
    public class PaletteAsset : ScriptableObject
    {
        [Header("Palette Conversion Table Generation Settings")]
        public PaletteDistanceMetric distanceMetric = PaletteDistanceMetric.CIE76;

        public List<Color> colors = new List<Color>();
        [HideInInspector] public Texture3D bakedLUT3D;

        public void UpdateBakedLUT(Texture3D lut)
        {
            if (bakedLUT3D != null && bakedLUT3D != lut)
            {
                DestroyTexture(bakedLUT3D);
            }
            bakedLUT3D = lut;
        }

        private void OnDestroy()
        {
            if (bakedLUT3D != null)
            {
                DestroyTexture(bakedLUT3D);
            }
        }

        private void DestroyTexture(Texture3D tex)
        {
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