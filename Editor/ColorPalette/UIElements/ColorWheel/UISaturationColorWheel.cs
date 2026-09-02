using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UISaturationColorWheel.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UISaturationColorWheel : UIBaseColorWheel
    {
        protected override float ShaderWheelTypeValue => 0.0f;

        public UISaturationColorWheel(ColorWheelCache cache) : base(cache) { }

        protected override Vector2 GetMarkerPosition(int index, Rect wheelRect)
        {
            return Cache.GetWheelPosition(index, wheelRect, isValueWheel: false);
        }

        protected override bool TryFindMarker(Vector2 localPos, Rect wheelRect, int selectedIndex, float radius, out int foundIndex)
        {
            return Cache.TryFindMarkerAtPosition(localPos, wheelRect, isValueWheel: false, selectedIndex, radius, out foundIndex);
        }

        protected override void ProcessDragInput(Vector2 offset, float magnitude, float hue, ref Vector3 hsv, float startSaturation)
        {
            if (magnitude > 0.0001f)
            {
                hsv.x = hue;
            }
            hsv.y = magnitude;
        }

        protected override void ProcessShiftDragInput(float delta, ref Vector3 hsv, float startSaturation, float startValue)
        {
            hsv.y = Mathf.Clamp(startSaturation - delta, 0.0001f, 0.9999f);
        }
    }
}