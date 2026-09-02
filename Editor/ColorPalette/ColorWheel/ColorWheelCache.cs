using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    ColorWheelCache.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class ColorWheelCache
    {
        private readonly List<Vector3> _hsvCache = new List<Vector3>();
        private readonly List<Color> _rgbCache = new List<Color>();

        public int CachedCount => _hsvCache.Count;

        public Vector3 GetHSV(int index) => _hsvCache[index];

        public Color GetRGB(int index) => _rgbCache[index];

        public void SetHSV(int index, Vector3 value)
        {
            _hsvCache[index] = value;
            _rgbCache[index] = Color.HSVToRGB(value.x, value.y, value.z);
        }

        public void Sync(List<Color> colors)
        {
            if (colors == null) return;

            if (_hsvCache.Count != colors.Count)
            {
                _hsvCache.Clear();
                _rgbCache.Clear();
                for (int i = 0; i < colors.Count; i++)
                {
                    Color col = colors[i];
                    Color.RGBToHSV(col, out float h, out float s, out float v);
                    _hsvCache.Add(new Vector3(h, s, v));
                    _rgbCache.Add(col);
                }
                return;
            }

            for (int i = 0; i < colors.Count; i++)
            {
                Color currentRGB = colors[i];
                if (currentRGB != _rgbCache[i])
                {
                    Color.RGBToHSV(currentRGB, out float h, out float s, out float v);

                    if (v == 0) 
                    { 
                        h = _hsvCache[i].x; 
                        s = _hsvCache[i].y;
                    }

                    else if (s == 0) 
                    { 
                        h = _hsvCache[i].x; 
                    }

                    _hsvCache[i] = new Vector3(h, s, v);
                    _rgbCache[i] = currentRGB;
                }
            }
        }

        public Vector2 GetWheelPosition(int index, Rect targetRect, bool isValueWheel)
        {
            Vector3 hsv = _hsvCache[index];
            float angle = hsv.x * 2.0f * Mathf.PI;
            float magnitude = isValueWheel ? hsv.z : hsv.y;
            float radius = magnitude * ((targetRect.width * 0.5f) - 1f);
            return targetRect.center + new Vector2(Mathf.Cos(angle) * radius, -Mathf.Sin(angle) * radius);
        }

        public bool TryFindMarkerAtPosition(
            Vector2 mousePos, 
            Rect targetRect, 
            bool isValueWheel, 
            int selectedIndex, 
            float radius, 
            out int foundIndex
        )
        {
            if (selectedIndex >= 0 && selectedIndex < _hsvCache.Count)
            {
                if (Vector2.Distance(mousePos, GetWheelPosition(selectedIndex, targetRect, isValueWheel)) <= radius)
                {
                    foundIndex = selectedIndex;
                    return true;
                }
            }

            for (int i = 0; i < _hsvCache.Count; i++)
            {
                if (i == selectedIndex) continue;
                if (Vector2.Distance(mousePos, GetWheelPosition(i, targetRect, isValueWheel)) <= radius)
                {
                    foundIndex = i;
                    return true;
                }
            }

            foundIndex = -1;
            return false;
        }
    }
}