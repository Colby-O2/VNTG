
//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXPaletteInputMode.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    /// <summary>
    /// Defines how the user wants to input a color palette to the PSX shader.
    /// </summary>
    public enum PSXPaletteInputMode
    {
        /// <summary>
        /// Color palette texture.
        /// </summary>
        Texture,
        /// <summary>
        /// User inputted list of colors.
        /// </summary>
        ColorList,
        /// <summary>
        /// Hex file TextAsset.
        /// </summary>
        HexFile
    }
}
