using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    GUIHelpers.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class GUIHelpers
    {
        public static Rect PadRect
        (
            Rect rect, 
            float left, 
            float right, 
            float top, 
            float bottom
        )
        {
            return new Rect(
                rect.x + left,
                rect.y + top,
                rect.width - left - right,
                rect.height - top - bottom
            );
        }

        public static Rect PadRect(Rect rect, float padding)
        {
            return PadRect(rect, padding, padding, padding, padding);
        }
    }
}
