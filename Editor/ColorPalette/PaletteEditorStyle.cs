using UnityEngine;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteEditorStyle.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette.Editor
{
    public static class PaletteEditorStyle
    {
        public static readonly Color HighlightColor = new Color(0.4f, 0.8f, 1f, 1f);
        public static readonly Color BorderColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        public static readonly Color FoldColorUpper = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        public static readonly Color FoldColorLower = new Color(0.4f, 0.8f, 1f, 1f);
        public static readonly Color HandleRectColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);
        public static readonly Color HandleIconColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        public const float RectSize = 36.0f;
        public const float BorderSize = 1.0f;
        public const float HighlightBorderSize = 2.0f;
        public const float FoldSize = 8.0f;
        public const float FoldPadding = 2.0f;

        public const float WheelSize = 300.0f;
        public const float BarWidth = 22.5f;
        public const float Spacing = 15.0f;
        public const float MarkerSize = 8.0f;
        public const float KnobSize = 16.0f;
    }
}