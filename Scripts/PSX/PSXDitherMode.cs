//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXDitherMode.cs
//-----------------------------------------------------------------------

namespace ColbyO.VNTG.PSX
{
    /// <summary>
    /// Defines the dithering method used to reduce color banding and simulate higher bit-depth.
    /// </summary>
    public enum PSXDitherMode
    {
        /// <summary>
        /// Dithering is applied based on the overall brightness of the scene.
        /// </summary>
        MultiplicativeLuminance = 0,

        /// <summary>
        /// Dithering is applied per color channel.
        /// </summary>
        MultiplicativeChannel = 1,

        /// <summary>
        /// Dithering shifts pixel values based on the dither pattern and color precision.
        /// </summary>
        Additive = 2
    }
}