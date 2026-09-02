using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    UIValueColorWheel.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public class UIValueColorWheel : UIBaseColorWheel
    {
        protected override float ShaderWheelTypeValue => 1.0f;

        public UIValueColorWheel(ColorWheelCache cache) : base(cache)
        {
            AddToClassList("color-picker__wheel--value-mode");
            style.marginLeft = PaletteEditorStyle.Spacing;
        }

        protected override Vector2 GetMarkerPosition(int index, Rect wheelRect)
        {
            return Cache.GetWheelPosition(index, wheelRect, isValueWheel: true);
        }

        protected override bool TryFindMarker(Vector2 localPos, Rect wheelRect, int selectedIndex, float radius, out int foundIndex)
        {
            return Cache.TryFindMarkerAtPosition(localPos, wheelRect, isValueWheel: true, selectedIndex, radius, out foundIndex);
        }

        protected override void ProcessDragInput(Vector2 offset, float magnitude, float hue, ref Vector3 hsv, float startSaturation)
        {
            if (magnitude > 0.0001f)
            {
                hsv.x = hue;
                hsv.y = startSaturation;
            }
            hsv.z = Mathf.Clamp(magnitude, 0.0001f, 1.0f);
        }

        protected override void ProcessShiftDragInput(float delta, ref Vector3 hsv, float startSaturation, float startValue)
        {
            hsv.z = Mathf.Clamp(startValue - delta, 0.0001f, 0.9999f);
        }
    }
}