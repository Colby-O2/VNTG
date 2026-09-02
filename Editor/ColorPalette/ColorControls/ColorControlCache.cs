using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    ColorControlCache.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class ColorControlCache
    {
        private ColorCache _cache = new ColorCache();

        public void Clear()
        {
            _cache.LastColor = new Color(-1f, -1f, -1f, -1f);
            _cache.HasCachedData = false;
        }

        public void GetHSV(Color inputColor, out float h, out float s, out float v)
        {
            if (_cache.HasCachedData && ColorsMatch(inputColor, _cache.LastColor))
            {
                h = _cache.Hue;
                s = _cache.Saturation;
                v = _cache.Value;
                return;
            }

            Color.RGBToHSV(inputColor, out h, out s, out v);

            _cache.Hue = h;
            _cache.Saturation = s;
            _cache.Value = v;
            _cache.LastColor = inputColor;
            _cache.HasCachedData = true;
        }

        public void UpdateHSV(float h, float s, float v, Color resultingColor)
        {
            _cache.Hue = h;
            _cache.Saturation = s;
            _cache.Value = v;
            _cache.LastColor = resultingColor;
            _cache.HasCachedData = true;
        }

        public void GetHSL(Color inputColor, out float h, out float s, out float l)
        {
            if (_cache.HasCachedData && ColorsMatch(inputColor, _cache.LastColor))
            {
                h = _cache.HslHue;
                s = _cache.HslSaturation;
                l = _cache.Ligntness;
                return;
            }

            inputColor.RGBToHSL(out h, out s, out l);

            _cache.HslHue = h;
            _cache.HslSaturation = s;
            _cache.Ligntness = l;
            _cache.LastColor = inputColor;
            _cache.HasCachedData = true;
        }

        public void UpdateHSL(float h, float s, float l, Color resultingColor)
        {
            _cache.HslHue = h;
            _cache.HslSaturation = s;
            _cache.Ligntness = l;
            _cache.LastColor = resultingColor;
            _cache.HasCachedData = true;
        }

        public void GetLab(Color inputColor, out float l, out float a, out float b)
        {
            if (_cache.HasCachedData && ColorsMatch(inputColor, _cache.LastColor))
            {
                l = _cache.LabL;
                a = _cache.LabA;
                b = _cache.LabB;
                return;
            }

            inputColor.RGBToLab(out l, out a, out b);

            _cache.LabL = l;
            _cache.LabA = a;
            _cache.LabB = b;
            _cache.LastColor = inputColor;
            _cache.HasCachedData = true;
        }

        public void UpdateLab(float l, float a, float b, Color resultingColor)
        {
            _cache.LabL = l;
            _cache.LabA = a;
            _cache.LabB = b;
            _cache.LastColor = resultingColor;
            _cache.HasCachedData = true;
        }

        public void GetOklab(Color inputColor, out float l, out float a, out float b)
        {
            if (_cache.HasCachedData && ColorsMatch(inputColor, _cache.LastColor))
            {
                l = _cache.OkLabL;
                a = _cache.OkLabA;
                b = _cache.OkLabB;
                return;
            }

            inputColor.RGBToOklab(out l, out a, out b);

            _cache.OkLabL = l;
            _cache.OkLabA = a;
            _cache.OkLabB = b;
            _cache.LastColor = inputColor;
            _cache.HasCachedData = true;
        }

        public void UpdateOklab(float l, float a, float b, Color resultingColor)
        {
            _cache.OkLabL = l;
            _cache.OkLabA = a;
            _cache.OkLabB = b;
            _cache.LastColor = resultingColor;
            _cache.HasCachedData = true;
        }

        private bool ColorsMatch(Color c1, Color c2)
        {
            return Mathf.Abs(c1.r - c2.r) < 0.001f &&
                   Mathf.Abs(c1.g - c2.g) < 0.001f &&
                   Mathf.Abs(c1.b - c2.b) < 0.001f &&
                   Mathf.Abs(c1.a - c2.a) < 0.001f;
        }

        private struct ColorCache
        {
            public Color LastColor;
            public bool HasCachedData;

            public float Hue;
            public float Saturation;
            public float Value;

            public float HslHue;
            public float HslSaturation;
            public float Ligntness;

            public float LabL;
            public float LabA;
            public float LabB;

            public float OkLabL;
            public float OkLabA;
            public float OkLabB;
        }
    }
}