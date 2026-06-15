using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    ListColorParameter.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    [System.Serializable]
    public class ListColorParameter : VolumeParameter<System.Collections.Generic.List<Color>>
    {
        public ListColorParameter(System.Collections.Generic.List<Color> value, bool overrideState = false) : base(value, overrideState) { }
    }
}
