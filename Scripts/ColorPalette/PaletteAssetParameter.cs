using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteAssetParameter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette
{
    [System.Serializable]
    public class PaletteAssetParameter : VolumeParameter<Palette>
    {
        public PaletteAssetParameter(Palette value, bool overrideState = false) : base(value, overrideState) { }
    }
}
