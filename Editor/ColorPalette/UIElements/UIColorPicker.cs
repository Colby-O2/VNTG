using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIColorPicker.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIColorPicker : VisualElement
    {
        private readonly UIBaseColorWheel _wheel;
        private readonly ColorWheelCache _cache = new ColorWheelCache();
        private List<Color> _colors = new List<Color>() { Color.red };

        public event Action<int, Color> OnColorChanged;

        public UIColorPicker()
        {
            _wheel = new UISaturationColorWheel(_cache);
            Add(_wheel);

            _wheel.OnColorModified += (index, hsv) =>
            {
                _cache.SetHSV(index, hsv);
                Color updatedColor = _cache.GetRGB(index);
                _colors[index] = updatedColor;

                OnColorChanged?.Invoke(index, updatedColor);
                Refresh();
            };

            _wheel.OnHoverChanged += index => _wheel.HoveringIndex = index;

            RegisterCallback<GeometryChangedEvent>(e => Refresh());
            Refresh();
        }

        public void SetColors(List<Color> colors)
        {
            _colors = colors;
            Refresh();
        }

        private void Refresh()
        {
            _cache.Sync(_colors);
            Vector3 hsv = _cache.GetHSV(0);
            _wheel.UpdateVisuals(_colors, 0, hsv.z);
        }
    }
}