using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    TextAssetParameter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    [System.Serializable]
    public class TextAssetParameter : VolumeParameter<TextAsset>
    {
        public TextAssetParameter(TextAsset value, bool overrideState = false) : base(value, overrideState) { }
    }
}
