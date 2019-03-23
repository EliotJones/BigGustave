namespace BigGustave
{
    using System;

    /// <summary>
    /// Describes the interpretation of the image data.
    /// </summary>
    [Flags]
    public enum ColorType : byte
    {
        None = 0,
        PaletteUsed = 1,
        ColorUsed = 2,
        AlphaChannelUsed = 4
    }
}