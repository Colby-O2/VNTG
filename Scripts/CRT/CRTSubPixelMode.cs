//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    CRTSubPixelMode.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.CRT
{
    /// <summary>
    /// Defines the sub-pixel method for a CRTRendererPass.
    /// </summary>
    public enum CRTSubPixelMode
    {
        /// <summary>
        /// Raw pixels are displayed.
        /// </summary>
        None = 0,

        /// <summary>
        /// A honeycomb pxiel pattern.
        /// </summary>
        ShadowMask,

        /// <summary>
        /// Vertical wire strips.
        /// </summary>
        ApertureGrille,

        /// <summary>
        /// Rectangular pixel pattern.
        /// </summary>
        SpotMask
    }
}