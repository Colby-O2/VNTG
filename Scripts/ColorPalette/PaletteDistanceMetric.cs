//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PaletteDistanceMetric.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.ColorPalette
{
    public enum PaletteDistanceMetric
    {
        EuclideanRGB,
        Redmean,
        Luminance,
        Hue,
        CIE76,
        CIE94,
        CMC1984,
        CIEDE2000,
        HyAB,
        BT2124_ITP
    }
}